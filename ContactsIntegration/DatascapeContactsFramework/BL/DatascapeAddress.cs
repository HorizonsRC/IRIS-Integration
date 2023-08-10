using HRC.Contacts.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRC.DatascapeContacts.BL
{
    public class DatascapeAddress
    {
        public Address Base { get; set; }
        
        public string Country { get; set; }

        public string FormattedAddress()
        {
            string address = "";
            //if (Base.AddressTypeEnum == AddressType.Street || Base.AddressTypeEnum == AddressType.Rural)
            //{
            //    address += (String.IsNullOrEmpty(Base.UnitType) ? "": Base.UnitType + " ") + (String.IsNullOrEmpty(Base.UnitId) ? "" : Base.UnitId + " ") + (String.IsNullOrEmpty(Base.FloorType) ? "" : Base.FloorType + " ") + (String.IsNullOrEmpty(Base.FloorId) ? "" : Base.FloorId + " ");
            //    address = address == "" ? address.Trim() : address.Trim() + ", ";
            //    address += (String.IsNullOrEmpty(Base.BuildingPropertyName) ? "" : (Base.BuildingPropertyName + ", ")) +
            //               (Base.HouseNumber.ToString() ?? "") + (Base.StreetAlpha ?? "") + " " + Base.StreetName +
            //               (String.IsNullOrEmpty(Base.RuralDeliveryNumber) ? "" : ", RD" + Base.RuralDeliveryNumber);
            //}
            //else if (Base.AddressTypeEnum == AddressType.POBox || Base.AddressTypeEnum == AddressType.PrivateBag || Base.AddressTypeEnum == AddressType.OtherDelivery)
            //{
            //    address += Base.AddressLine1;
            //}
            //else if (Base.AddressTypeEnum == AddressType.Overseas)
            //{
            //    address += Base.AddressLine1 +
            //                (Base.AddressLine2 == null ? "" : (", " + Base.AddressLine2)) +
            //                (Base.AddressLine3 == null ? "" : (", " + Base.AddressLine3)) +
            //                (Base.AddressLine4 == null ? "" : (", " + Base.AddressLine4)) +
            //                (Base.AddressLine5 == null ? "" : (", " + Base.AddressLine5));
            //}

            //return address;
            if (Base.AddressTypeEnum == AddressType.Street || Base.AddressTypeEnum == AddressType.Rural)
            {
                if (!String.IsNullOrEmpty(Base.UnitType))
                    address += Base.UnitType + " ";

                if (!String.IsNullOrEmpty(Base.UnitId))
                    address += Base.UnitId + " ";

                if (!String.IsNullOrEmpty(Base.FloorType))
                    address += Base.FloorType + " ";

                if (!String.IsNullOrEmpty(Base.FloorId))
                    address += Base.FloorId + " ";

                address = address.Trim();

                if (!String.IsNullOrEmpty(address))
                    address += ", ";

                if (!String.IsNullOrEmpty(Base.BuildingPropertyName))
                    address += Base.BuildingPropertyName + ", ";

                address += Base.HouseNumber.ToString() + (Base.StreetAlpha ?? "") + " " + Base.StreetName;

                if (!String.IsNullOrEmpty(Base.RuralDeliveryNumber))
                    address += " RD " + Base.RuralDeliveryNumber;
            }
            else if (Base.AddressTypeEnum == AddressType.POBox || Base.AddressTypeEnum == AddressType.PrivateBag || Base.AddressTypeEnum == AddressType.OtherDelivery)
            {
                address += Base.AddressLine1;
            }
            else if (Base.AddressTypeEnum == AddressType.Overseas)
            {
                if (!String.IsNullOrEmpty(Base.AddressLine1))
                    address += Base.AddressLine1 + ", ";

                if (!String.IsNullOrEmpty(Base.AddressLine2))
                    address += Base.AddressLine2 + ", ";

                if (!String.IsNullOrEmpty(Base.AddressLine3))
                    address += Base.AddressLine3 + ", ";

                if (!String.IsNullOrEmpty(Base.AddressLine4))
                    address += Base.AddressLine4 + ", ";

                if (!String.IsNullOrEmpty(Base.AddressLine5))
                    address += Base.AddressLine5;
            }

            return address;

        }
    }
}
