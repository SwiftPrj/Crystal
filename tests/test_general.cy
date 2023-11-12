func readout: void -> 
{
    var a: int = 33;
    sysout -> a;
    var b: int = 444;
    sysout -> b;
}

func Main: int -> 
{
    var a: int = 500;
    sysout -> a;
    var b: int = 22;
    sysout -> b;

    readout();

    return 0;
}