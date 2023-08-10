using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.Common;

namespace HRC.Contacts.BL
{
    public enum AddressType : byte
    {
        Street = 0,
        Rural = 1,
        Overseas = 2,
        POBox = 3,
        PrivateBag = 4,
        OtherDelivery = 5,
    }

    public class Address : IdNameBase<int>
    {
        public string AddressType { get; set; }
                
        public AddressType AddressTypeEnum { get; set; }

        public string Prologue { get; set; }        

        public string RuralDeliveryNumber { get; set; }

        public string AddressNumberText { get; set; }
        public string StreetAlpha { get; set; }
        public int HouseNumber { get; set; }

        public string FloorType { get; set; }
        public string FloorId { get; set; }
                
        public string BuildingPropertyName { get; set; }
        public string UnitType { get; set; }
        public string UnitId { get; set; }

        public string StreetName { get; set; }
        public string StreetType { get; set; }
        public string StreetSuffix { get; set; }
        public string Suburb { get; set; }
        public string TownLocality { get; set; }
        public string PostalCode { get; set; }
        public bool? PrimaryPhysicalFlag { get; set; }
        public bool? PrimaryPostalFlag { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string AddressLine5 { get; set; }
        public string AddressLine6 { get; set; }

        public int ContactId { get; set; }  //PowerBuilder [cont_person].[Agent_Id]
        public long SourceId { get; set; }  //Iris         [Address].[Id]
        
        public bool IsBilling { get; set; }
        public bool IsCareOf { get; set; }
        public bool IsPostal { get; set; }
        public bool IsCurrent { get; set; }
    }
}
