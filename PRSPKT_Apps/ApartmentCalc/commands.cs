using System;
using System.Collections.Generic;
using System.Linq;

namespace PRSPKT_Apps.ApartmentCalc
{
    class commands
    {
        UIDocument uidoc = this.ActiveUIDocument;
        Document doc = uidoc.Document;
		using (Transaction t = new Transaction(doc, "Квартирография")) {
				t.Start();
				string msg = "";

    IList<Room> ModelRooms = new FilteredElementCollector(doc, uidoc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rooms).Cast<Room>().ToList();

    int roundCount = 2; // Округлить до __ знаков

    var query =
        from element in ModelRooms
            //where element.Level.Name == lookingFor
        where element.LookupParameter("Тип помещения").AsInteger() != 5
        where element.Area > 0
        group element by element.LookupParameter("Номер квартиры").AsString() into apartNumber
        from room in apartNumber
        group room by apartNumber.Key;
				
				foreach (var _apart in query)
				{
					msg += _apart.Key + " \r\n";
					List<double> area_list = ApartAreas(_apart.ToList(), roundCount);
    double area_L = area_list[0]; // Жилая Площадь
    double area_L_converted = UnitUtils.ConvertToInternalUnits(area_L, DisplayUnitType.DUT_SQUARE_METERS);
    double area_A = area_list[1]; // Площадь Квартиры
    double area_A_converted = UnitUtils.ConvertToInternalUnits(area_A, DisplayUnitType.DUT_SQUARE_METERS);
    double area_C = area_list[2]; // Общая Площадь
    double area_C_converted = UnitUtils.ConvertToInternalUnits(area_C, DisplayUnitType.DUT_SQUARE_METERS);
    int count_r = ApartCountRoom(_apart.ToList()); // Количество жилых комнат

					// Назначаем параметры для каждого помещения
					foreach (var _room in _apart) {
						Parameter Area_L = _room.LookupParameter("Площадь квартиры Жилая");
    Parameter Area_A = _room.LookupParameter("Площадь квартиры");
    Parameter Area_C = _room.LookupParameter("Площадь квартиры Общая");
    Parameter Count_R = _room.LookupParameter("Число комнат");
						try {
							int _type = _room.LookupParameter("Тип помещения").AsInteger();
    Area_L.Set(area_L_converted);
							Area_A.Set(area_A_converted);
							Area_C.Set(area_C_converted);
							Count_R.Set(count_r);
							
						} catch (Exception e) {
							
							TaskDialog.Show("Квартирография", "Ошибка: " + e.Message);
						}
					}
					
					msg += String.Format("Жилая площадь: {0} \r\n " +
										 "Площадь квартиры: {2} \r\n " +
										 "Общая площадь: {3} \r\n " +
										 "Число комнат: {1} \r\n" +
										 "\r\n",
										 area_L.ToString(),
										 count_r.ToString(),
										 area_A.ToString(),
										 area_C.ToString()
										);
				}
				t.Commit();
				TaskDialog.Show("Message", msg);
			}
		}
		private const double METERS_IN_FEET = 0.3048;

private List<double> ApartAreas(IList<Room> rms, int round)
{

    double room_living_sum = 0;
    double room_apart_sum = 0;
    double room_common_sum = 0;
    List<double> outlist = new List<double>();

    foreach (Room r in rms)
    {
        int _type = r.LookupParameter("Тип помещения").AsInteger();
        double koef = RoomKoef(_type);
        double areaC = UnitUtils.ConvertFromInternalUnits(r.Area, DisplayUnitType.DUT_SQUARE_METERS);
        double area = Math.Round(areaC, round);
        double karea = Math.Round(areaC * koef, round);
        double karea_converted = UnitUtils.ConvertToInternalUnits(karea, DisplayUnitType.DUT_SQUARE_METERS);
        double area_converted = UnitUtils.ConvertToInternalUnits(area, DisplayUnitType.DUT_SQUARE_METERS);
        switch (_type)
        {
            case 1:
                room_living_sum += area;
                room_apart_sum += area;
                break;
            case 2:
                room_apart_sum += area;
                break;
        }
        room_common_sum += karea;

        r.LookupParameter("Площадь с коэффициентом").Set(karea_converted);
        r.LookupParameter("Коэффициент площади").Set(koef);
    }
    outlist.Add(room_living_sum);
    outlist.Add(room_apart_sum);
    outlist.Add(room_common_sum);
    return outlist;
}

private int ApartCountRoom(IList<Room> rms)
{
    int _count = 0;
    foreach (Room r in rms)
    {
        int _type = r.LookupParameter("Тип помещения").AsInteger();
        if (_type == 1)
        {
            _count += 1;
        }
    }
    return _count;
}

/*private double AcceptKoef(int type, double area, int round)
{
	return Math.Round(RoomKoef(type) * Math.Round(area * 0.09290304, round), round);
	//return Math.Round(RoomKoef(type) * Math.Round(area, round), round);
}*/

private double RoomKoef(int type)
{
    double k;
    switch (type)
    {
        case 5:
            k = 0;
            break;
        case 3:
            k = 0.5;
            break;
        case 4:
        case 6:
            k = 0.3;
            break;
        default:
            k = 1;
            break;
    }
    return k;
}
	}
	}
}
