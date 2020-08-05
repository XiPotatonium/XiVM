function Build-Module {
    param ($moduleName)
    ../../XiLang/bin/Debug/netcoreapp3.1/XiLang.exe $moduleName -verbose
}

function Invoke-Module {
    param ($path)
    ../../XiVM/bin/Debug/netcoreapp3.1/XiVM.exe $path
}

Build-Module Test0
Invoke-Module Test0.xibc

Build-Module GCD
Invoke-Module GCD.xibc

Build-Module HelloWorld
Invoke-Module HelloWorld.xibc