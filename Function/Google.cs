using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace optanaPCI.Function
{
    class Google
    {        
        public class GoogleMapServices
        {
            string MapUrl = "https://maps.googleapis.com/maps/api/geocode/json?latlng=";
            string Mykey = "&key=AIzaSyCbNmgX7fnZCkPHnFlruXYVit1HrdJioe4";

            // 傳入經緯度,取得住址
            // Lat = 緯度, Lng = 經度
            public string GetAddressByLatLng(string Lng, string Lat)
            {
                string strResult = "";
                GoogleGeoCodeResponse _mapdata = ConvertLatLngToAddress(Lng, Lat);
                if (_mapdata.status == "OK")
                {
                    try
                    {
                        strResult = _mapdata.results[0].formatted_address;
                    }
                    catch
                    {

                    }
                }
                return strResult;
            }

            public GoogleGeoCodeResponse ConvertLatLngToAddress(string Lng, string Lat)
            {
                string result = string.Empty;

                string url = MapUrl + "{0},{1}";
                url = string.Format(url, Lng, Lat);
                url += Mykey;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                using (var response = request.GetResponse())
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {

                    result = sr.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(result);
            }

            // 以住址查詢,回傳經緯度
            /// <param name="addr"></param>
            public location GetLatLngByAddr(string addr)
            {
                location _result = new location();
                GoogleGeoCodeResponse _mapdata = new GoogleGeoCodeResponse();
                _mapdata = ConvertAddressToLatLng(addr);
                if (_mapdata.status == "OK")
                {
                    try
                    {
                        _result.lat = _mapdata.results[0].geometry.location.lat;
                        _result.lng = _mapdata.results[0].geometry.location.lng;
                    }
                    catch
                    {

                    }
                }
                return _result;
            }

            public GoogleGeoCodeResponse ConvertAddressToLatLng(string addr)
            {
                string result = string.Empty;

                //string googlemapkey = "AIzaSyBGLwRnlbH6f3NYeXJFlxPT-FUdrHpv_O4";
                string url = MapUrl + "&address={0}";
                url = string.Format(url, addr);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                using (var response = request.GetResponse())
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {

                    result = sr.ReadToEnd();
                }

                return JsonConvert.DeserializeObject<GoogleGeoCodeResponse>(result);
            }
            
            public class GoogleGeoCodeResponse
            {
                public string status { get; set; }
                public results[] results { get; set; }
            }
            
            public class results
            {
                public string formatted_address { get; set; }
                public geometry geometry { get; set; }
                public string[] types { get; set; }
                public address_component[] address_components { get; set; }
            }

            public class geometry
            {
                public string location_type { get; set; }
                public location location { get; set; }
            }

            public class location
            {
                public string lat { get; set; }
                public string lng { get; set; }
            }

            public class address_component
            {
                public string long_name { get; set; }
                public string short_name { get; set; }
                public string[] types { get; set; }
            }
        }

        public class MapDistanceServices
        {
            public MapDistanceServices()
            {
            }
            private const double EARTH_RADIUS = 6378.137;
            private double rad(double d)
            {
                return d * Math.PI / 180.0;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="lat1">緯度1</param>
            /// <param name="lng1">經度1</param>
            /// <param name="lat2">緯度2</param>
            /// <param name="lng2">經度2</param>
            /// <returns></returns>
            public double GetDistance(double lat1, double lng1, double lat2, double lng2)
            {
                double dblResult = 0;
                double radLat1 = rad(lat1);
                double radLat2 = rad(lat2);
                double distLat = radLat1 - radLat2;
                double distLng = rad(lng1) - rad(lng2);
                dblResult = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(distLat / 2), 2) +
                                Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(distLng / 2), 2)));
                dblResult = dblResult * EARTH_RADIUS;
                //dblResult = Math.Round(dblResult * 10000) /10000;  //這回傳變成公里,少3個0變公尺
                dblResult = Math.Round(dblResult * 10000) / 10;

                return dblResult;
            }
        }
    }
}
