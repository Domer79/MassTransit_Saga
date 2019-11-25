#[Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding("cp866")

$vs_instance = Get-VSSetupInstance
$buildPath = $vs_instance.InstallationPath + "\MSBuild\Current\Bin\MSBuild.exe"
& $buildPath 'DataBusService.csproj' '/p:Configuration=Debug'
& $buildPath 'DataBusService.csproj' '/p:Configuration=Debug_net461'
& ".\nuget.exe" 'pack' '.\nuget_package\MassTransit.DataBusService.nuspec' '-OutputDirectory' '.\nuget_package' '-Verbosity' 'detailed'