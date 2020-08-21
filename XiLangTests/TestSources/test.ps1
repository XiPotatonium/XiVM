function Build-Module {
    param ($moduleName)
    ../../XiLang/bin/Debug/netcoreapp3.1/XiLang.exe $moduleName -verbose
}

function Invoke-Module {
    param ($moduleName)
    ../../XiVM/bin/Debug/netcoreapp3.1/XiVM.exe $moduleName -diagnose
}

Copy-Item ../../SystemLib/bin/Debug/netcoreapp3.1/System.xibc System.xibc

Write-Output "Running HelloWorld.xi:"
Build-Module HelloWorld
Invoke-Module HelloWorld

Write-Output "Running Test0.xi:"
Build-Module Test0
Invoke-Module Test0

Write-Output "Running Test1.xi:"
Build-Module Test1
Invoke-Module Test1