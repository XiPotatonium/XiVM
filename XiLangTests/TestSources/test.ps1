function Build-Module {
    param ($moduleName)
    ../../XiLang/bin/Debug/netcoreapp3.1/XiLang.exe $moduleName -verbose
}

function Invoke-Module {
    param ($moduleName)
    ../../XiVM/bin/Debug/netcoreapp3.1/XiVM.exe $moduleName -diagnose
}

Copy-Item ../../SystemLib/bin/Debug/netcoreapp3.1/System.xibc System.xibc

Build-Module Test0
Invoke-Module Test0

Build-Module Test1
Invoke-Module Test1

Build-Module HelloWorld
Invoke-Module HelloWorld