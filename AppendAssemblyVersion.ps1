$gitRevision = (git rev-parse HEAD | Out-String)
$gitBranch   = (git rev-parse --abbrev-ref HEAD | Out-String)
$metadata = "using System.Reflection;`r`n`r`n[assembly: AssemblyMetadata(""CommitHash"", ""{0}"")]`r`n[assembly: AssemblyMetadata(""CommitBranch"", ""{1}"")]" -f $gitRevision.Trim(), $gitBranch.Trim()
$metadata | Out-File -FilePath .\AssemblyVersion.cs -Append -Encoding utf8 -Force
