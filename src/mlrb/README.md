### Fix all Dapper.AOT errors by puring all bin &amp; and obj directories:

* Powershell &mdash;
  `Get-ChildItem . -include bin,obj -Recurse | ForEach-Object { Remove-Item $_.FullName -Force -Recurse }`
* Bash/Zsh &mdash; `find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null || del /s /q bin` and then
  `find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null || del /s /q obj`
