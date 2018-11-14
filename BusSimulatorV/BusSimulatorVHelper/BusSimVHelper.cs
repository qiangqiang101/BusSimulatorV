using GTA;

namespace BusSimulatorVHelper
{
    public static class BusSimVHelper
    {
        public static unsafe void SetVehiclePassengerCount(this Vehicle vehicle, int passengerCount)
        {
            *(byte*)(vehicle.MemoryAddress + 0x0BD2) = (byte)passengerCount;
            //var vehAddr = (ulong)vehicle.MemoryAddress;
            //*(uint*)(vehAddr + 0x0BD2) = (uint)passengerCount;
        }
    }
}
