#[Console]::OutputEncoding = [System.Text.Encoding]::GetEncoding("cp866")

$vs_instance = Get-VSSetupInstance
$buildPath = $vs_instance.InstallationPath + "\MSBuild\15.0\bin\MSBuild.exe"
& $buildPath 'c:\Users\Domer\source\repos\ExampleProjects\MassTransit_Saga\DataBusService\DataBusService.csproj' '/p:Configuration=Debug'
& $buildPath 'c:\Users\Domer\source\repos\ExampleProjects\MassTransit_Saga\DataBusService\DataBusService.csproj' '/p:Configuration=Debug_net461'
nuget pack .\nuget_package\MassTransit.DataBusService.nuspec -OutputDirectory .\nuget_package