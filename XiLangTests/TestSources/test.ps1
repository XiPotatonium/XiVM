function Build-Module {
    param ($moduleName)
    ../../XiLang/bin/Debug/netcoreapp3.1/XiLang.exe $moduleName -verbose
}

function Invoke-Module {
    param ($moduleName)
    ../../XiVM/bin/Debug/netcoreapp3.1/XiVM.exe $moduleName
}

Build-Module Test0
Invoke-Module Test0

# Build-Module GCD
# Invoke-Module GCD.xibc

# Build-Module HelloWorld
# Invoke-Module HelloWorld.xibc