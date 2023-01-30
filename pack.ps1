$x = Split-Path -Parent $MyInvocation.MyCommand.Definition
cd $x 

dotnet build --configuration Release --framework net6.0
 
Get-ChildItem ./src/ -recurse *.nupkg | Remove-Item
dotnet pack -c Release --no-restore --no-build --include-symbols --include-source  
$pkgs = Get-ChildItem ./src/ -recurse *.nupkg;

foreach($pkg in $pkgs)
{
  	dotnet nuget push $pkg.FullName -n true --skip-duplicate
}
$spkgs = Get-ChildItem ./src/ -recurse *.snupkg;
foreach($spkg in $spkgs)
{
  	dotnet nuget push $spkg.FullName -n true --skip-duplicate
}
