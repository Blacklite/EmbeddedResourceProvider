$key = Get-Content api.key
$packages = Get-ChildItem -Recurse ResourceProvider.AspNet\*.nupkg
foreach ($pkg in $packages) {
	.\.nuget\nuget push $pkg.FullName $key
}
$packages = Get-ChildItem -Recurse ResourceProvider.Core\*.nupkg
foreach ($pkg in $packages) {
	.\.nuget\nuget push $pkg.FullName $key
}