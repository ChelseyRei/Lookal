# Lookal

<!-- DB CONNECTION TROUBLESHOOT -->
## Troubleshooting: PostgreSQL password errors

Use this section when you see any of these:

- `PostgresException 28P01: password authentication failed for user "postgres"`
- `psql: FATAL: password authentication failed for user "postgres"`
- pgAdmin rejects the password

The server is running, but it doesn't accept the password the app or pgAdmin is using.

### Causes

- The server password changed, for example after a reinstall, but your user secret still has the old one.
- pgAdmin has an outdated saved password.
- An edit to `pg_hba.conf` didn't save.

A `connection refused` error means something else: the PostgreSQL service isn't running. Start it with:

```powershell
Start-Service postgresql-x64-19
```

### 1. Check the basics

Confirm the service is running:

```powershell
Get-Service postgresql*
```

Confirm the port accepts connections:

```powershell
Test-NetConnection -ComputerName localhost -Port 5432
```

Look for `TcpTestSucceeded : True`.

### 2. Compare the passwords

Test the server password directly. Type the password when prompted; the characters won't show:

```powershell
& "C:\Program Files\PostgreSQL\19\bin\psql.exe" -U postgres -h localhost -d postgres
```

- `postgres=#` means the password works. Type `\q` to exit, then go to step 4.
- `password authentication failed` means the server password doesn't match. Go to step 3.

Check the password the app uses. Don't share this output, because it shows the password:

```powershell
dotnet user-secrets list --project src/Lookal.Web
```

### 3. Reset the server password to match the secret

Run this in PowerShell as Administrator. Replace `YOUR_SECRET_PASSWORD` with the value after `Password=`. Don't use single quotes in the password.

The script temporarily allows local connections without a password, sets the password, and restores the password check, even if an earlier step fails.

```powershell
$pgBin       = "C:\Program Files\PostgreSQL\19\bin"
$hba         = "C:\Program Files\PostgreSQL\19\data\pg_hba.conf"
$service     = "postgresql-x64-19"
$newPassword = "YOUR_SECRET_PASSWORD"

Copy-Item $hba "${hba}.bak" -Force

try {
    (Get-Content $hba) `
        -replace '(host\s+all\s+all\s+127\.0\.0\.1/32\s+)scram-sha-256', '$1trust' `
        -replace '(host\s+all\s+all\s+::1/128\s+)scram-sha-256', '$1trust' |
        Set-Content $hba

    Restart-Service $service
    & "$pgBin\psql.exe" -U postgres -h localhost -d postgres -c "ALTER USER postgres WITH PASSWORD '$newPassword';"
}
finally {
    (Get-Content $hba) `
        -replace '(host\s+all\s+all\s+127\.0\.0\.1/32\s+)trust', '$1scram-sha-256' `
        -replace '(host\s+all\s+all\s+::1/128\s+)trust', '$1scram-sha-256' |
        Set-Content $hba

    Restart-Service $service
    Write-Host "pg_hba.conf restored and service restarted."
}
```

Expected output: `ALTER ROLE`, then `pg_hba.conf restored and service restarted.`

Before running the script, confirm both local rules say `scram-sha-256`:

```powershell
Select-String -Path "C:\Program Files\PostgreSQL\19\data\pg_hba.conf" -Pattern "^host"
```

If the script fails at `ALTER ROLE` with `password authentication failed`, the rules weren't changed. Check the output of the command above.

Don't save the script with your real password in it. Clear the value after you run it.

Then repeat the test from step 2.

### 4. Fix the app or pgAdmin

If psql accepts the password but the app still fails:

1. Stop the app with `Ctrl + C`.
2. Confirm `appsettings.json` contains no real password. The connection string belongs only in user secrets.
3. Start the app again. A running copy doesn't read changed secrets.

If pgAdmin fails, right-click the server, choose **Disconnect Server**, then connect again and enter the password. To save it, open **Properties → Connection**, enter the correct password, and check **Save Password**.

### 5. Use a new password instead (optional)

Use this only if you don't need to keep the existing password.

1. Choose a new password with no single quotes.
2. Run the step 3 script with `$newPassword` set to the new value.
3. Update the secret:

```powershell
dotnet user-secrets set "ConnectionStrings:Default" 'Host=localhost;Database=lookal_local;Username=postgres;Password=YOUR_NEW_PASSWORD' --project src/Lookal.Web
```

4. Update the password saved in pgAdmin.
5. Restart the app.

### Prevention

- Save the `postgres` password in a password manager when you install PostgreSQL.
- Before reinstalling PostgreSQL, write down the password and make sure your secret matches it.
- Test with psql before running the app. This separates a server problem from an app problem.
- After any change to `pg_hba.conf`, confirm both local rules say `scram-sha-256`.
- Each teammate sets their own local password and secret. Secrets never go into Git.

### Common mistakes

- Editing `pg_hba.conf` in Notepad without administrator rights. The save can fail silently, so use the script.
- Leaving `trust` in place. It lets anyone on your computer connect without a password.
- Blaming the app's code for a password error. The server rejects the password before the app's code runs.
- Putting special characters in the password without checking how PowerShell handles them.