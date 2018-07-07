using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VehicleControls
{
    public class VehicleControls : BaseScript
    {
        private static string ERROR = "~r~Error: ";
        private static string ERROR_NOCAR = ERROR + "Du bist weder in einem Fahrzeug noch hast du eine gespeichertes Fahrzeug.";

        private Vehicle savedVehicle;

        private void AddEngineItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Moter an-/ausschalten");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    ToggleEngine(car);
                }
                else if (savedVehicle != null)
                {
                    ToggleEngine(savedVehicle);
                }
            };
        }

        private void ToggleEngine(Vehicle car)
        {
            if (car.IsEngineRunning)
            {
                Screen.ShowNotification("Motor ist jetzt ~r~aus~w~.");
                car.IsDriveable = false;
                car.IsEngineRunning = false;
            }
            else
            {
                Screen.ShowNotification("Motor ist jetzt ~g~an~w~.");
                car.IsDriveable = true;
                car.IsEngineRunning = true;
            }
        }

        private void AddDoorLockItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Türen auf-/absperren", "Achtung: Dadurch wird dieses Fahrzeug für dich gespeichert.");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == newItem)
                {
                    Vehicle car = LocalPlayer.Character.CurrentVehicle;

                    if (car == null
                     && savedVehicle == null)
                    {
                        Screen.ShowNotification(ERROR_NOCAR);
                        return;
                    }

                    if (car != null)
                    {
                        LockDoor(car);
                        SaveVehicle(car);
                    }
                    else
                    {
                        LockDoor(savedVehicle);
                    }
                }
            };
        }

        private void LockDoor(Vehicle car)
        {
            bool doorLocked = (Function.Call<int>(Hash.GET_VEHICLE_DOOR_LOCK_STATUS, car) == 2);

            if (doorLocked)
            {
                Screen.ShowNotification("Das Fahrzeug ist jetzt ~g~aufgesperrt~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 0);
            }
            else
            {
                Screen.ShowNotification("Das Fahrzeug ist jetzt ~r~abgesperrt~w~.");
                Function.Call(Hash.SET_VEHICLE_DOORS_LOCKED, car, 2);
            }
        }

        private void AddOpenDoorItem(UIMenu menu)
        {
            List<dynamic> doors = new List<dynamic>
            {
                "Vorne links",
                "Vorne rechts",
                "Hinten links",
                "Hinten rechts",
                "Moterhaube",
                "Kofferraum"
            };
            var newItem = new UIMenuListItem("Türen öffnen/schließen", doors, 0);
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                int itemIndex = newItem.Index;
                string doorName = newItem.IndexToItem(itemIndex);
                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    ToggleDoor(car, itemIndex, doorName);
                }
                else
                {
                    ToggleDoor(savedVehicle, itemIndex, doorName);
                }
            };
        }

        private void ToggleDoor(Vehicle car, int index, string doorName)
        {
            bool doorBroken = Function.Call<bool>(Hash.IS_VEHICLE_DOOR_DAMAGED, car, index);
            if (doorBroken)
            {
                Screen.ShowNotification(ERROR + "Die Tür ist kaputt.");
                return;
            }

            float doorAngle = Function.Call<float>(Hash.GET_VEHICLE_DOOR_ANGLE_RATIO, car, index);
            if (doorAngle == 0) // Door is closed
            {
                Screen.ShowNotification(doorName + " Tür ist jetzt ~g~offen~w~.");
                Function.Call(Hash.SET_VEHICLE_DOOR_OPEN, car, index, false, false);
            }
            else
            {
                Screen.ShowNotification(doorName + " Tür ist jetzt ~r~geschlossen~w~.");
                Function.Call(Hash.SET_VEHICLE_DOOR_SHUT, car, index, false);
            }
        }

        private void AddLockSpeedItem(UIMenu menu)
        {
            List<dynamic> speeds = new List<dynamic>()
            {
                "None"
            };
            for (int i = 10; i < 301; i = i + 10)
            {
                speeds.Add(i + " KM/H");
            }
            UIMenuListItem newItem = new UIMenuListItem("Max. Geschwindigkeit setzen", speeds, 0);
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null
                 && savedVehicle == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                if (car != null)
                {
                    LockSpeed(car, newItem);
                }
                else
                {
                    LockSpeed(savedVehicle, newItem);
                }
            };
        }

        private void LockSpeed(Vehicle car, UIMenuListItem item)
        {
            string[] itemName = item.IndexToItem(item.Index).Split(' ');
            if (itemName[0] == "None")
            {
                car.MaxSpeed = int.MaxValue;
                Screen.ShowNotification($"Geschwindigkeitsbegrenzung entfernt.");
                return;
            }

            float itemSpeed = float.Parse(itemName[0]) / 3.6f;
            car.MaxSpeed = itemSpeed;
            Screen.ShowNotification($"Geschwindigkeit wurde auf {itemName[0]} {itemName[1]}.");
        }

        private void AddSaveVehicleItem(UIMenu menu)
        {
            var newItem = new UIMenuItem("Fahrzeug speichern");
            menu.AddItem(newItem);

            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item != newItem)
                {
                    return;
                }

                Vehicle car = LocalPlayer.Character.CurrentVehicle;

                if (car == null)
                {
                    Screen.ShowNotification(ERROR_NOCAR);
                    return;
                }

                SaveVehicle(car);
                Screen.ShowNotification("Fahrzeug gespeichert.");
            };
        }

        private void SaveVehicle(Vehicle car)
        {
            if (savedVehicle != null)
            {
                foreach (Blip vehBlip in savedVehicle.AttachedBlips)
                {
                    vehBlip.Alpha = 0;
                }
            }

            Blip blip = car.AttachBlip();
            blip.Sprite = BlipSprite.PersonalVehicleCar;

            savedVehicle = car;
        }

        public VehicleControls()
        {
            MenuPool menuPool = new MenuPool();

            UIMenu menu = new UIMenu("Vehicle Controls", "");
            menuPool.Add(menu);

            AddEngineItem(menu);
            AddDoorLockItem(menu);
            AddOpenDoorItem(menu);
            AddLockSpeedItem(menu);
            AddSaveVehicleItem(menu);

            menu.RefreshIndex();

            Tick += new Func<Task>(async delegate
            {
                await Task.FromResult(0);

                menuPool.ProcessMenus();
                if (Game.IsControlJustReleased(1, Control.InteractionMenu))
                {
                    menu.Visible = !menu.Visible;
                }
            });
        }
    }
}
