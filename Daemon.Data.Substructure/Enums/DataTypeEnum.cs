using System.Collections.Generic;
using Daemon.Data.Substructure.Const;
using System;
using System.Linq;
namespace Daemon.Data.Substructure.Enums
{
    public enum DataTypeEnum : short
    {
        AlternateSite = 1,
        Contractor = 2,
        District = 3,
        FieldTrip = 4,
        GeoRegion = 5,
        GPSEvent = 6,
        School = 7,
        Staff = 8,
        Student = 9,
        Trip = 10,
        Vehicle = 11,
        Document = 12,
        TripStop = 13,
        TripHistory = 14,
        FieldTripTemplate = 15,
        FieldTripDestination = 16,
        RedistrictingBoundary = 17,
        ALL = 18,
        Contact = 19,
        Report = 20,

        ScheduledReport = 21,
        ReportLibrary = 22,
        Category = 23,
        Track = 24,
        Dashboard = 26,
        Other = 27,
        TrackSent = 28,
        TripSchedule = 29,
        StudentSchedule = 30,
        Route = 31,
        Form = 32,
        TripStopSchedule = 33,
        Session = 35,
        DashboardLibrary = 37,
        Customer = 100,
        Warranty = 101,
        Invoice = 102,
        Quote = 103,
        WorkOrder = 104,
        Part = 105,
        PurchaseOrder = 106,
        Location = 107,
        Order = 108,
        SFVendor = 109,
        SFVehicle = 110,
        Service = 111,
        Equipment = 112,
        SFStaff = 113,
        LaborAdjustment = 114,
        FormSent = 38,
        Forms = 40,
        MergeDocumentsSent = 41,
        ScheduledReportSent = 42,
        MergeDocument = 43,
        ScheduleResource = 115,
        ScheduleTechnician = 116,
        RepairCode = 117,
        Manufacturer = 118,
        AccountCodeTransaction = 119,
    }

    public static class DataTypeEnumExtension
    {
        private static readonly Dictionary<string, DataTypeEnum> _typeValueMap = Enum.GetValues(typeof(DataTypeEnum)).Cast<DataTypeEnum>().ToDictionary(i => i.ToDataTypeString(), StringComparer.OrdinalIgnoreCase);

        private static readonly IDictionary<DataTypeEnum, DataTableInfo> _dataTables = new Dictionary<DataTypeEnum, DataTableInfo>
        {
            { DataTypeEnum.AlternateSite, new DataTableInfo("AltSite", "AltSiteID") },
            { DataTypeEnum.Contractor, new DataTableInfo("Contractor", "Contractor_ID") },
            { DataTypeEnum.District, new DataTableInfo("District", "DistrictID") },
            { DataTypeEnum.FieldTrip, new DataTableInfo("FieldTrip", "FieldTripID") },
            { DataTypeEnum.GeoRegion, new DataTableInfo("GeoRegion", "GeoRegionID") },
            { DataTypeEnum.School, new DataTableInfo("School", "SchoolID") },
            { DataTypeEnum.Staff, new DataTableInfo("Staff", "Id") },
            { DataTypeEnum.Student, new DataTableInfo("Student", "Stud_ID") },
            { DataTypeEnum.Trip, new DataTableInfo("Trip", "TripID") },
            { DataTypeEnum.Document, new DataTableInfo("Document", "DocumentID ") },
            { DataTypeEnum.Customer, new DataTableInfo("Customer", "ID ") },
            { DataTypeEnum.Invoice, new DataTableInfo("Invoice", "ID ") },
            { DataTypeEnum.Quote, new DataTableInfo("Quote", "ID ") },
            { DataTypeEnum.TripStop, new DataTableInfo("TripStop", "TripStopID") },
            { DataTypeEnum.TripSchedule, new DataTableInfo("TripSchedule", "Id") },
            { DataTypeEnum.TripStopSchedule, new DataTableInfo("TripStopSchedule", "Id") },
            { DataTypeEnum.Vehicle, new DataTableInfo("Vehicle", "VehicleID") },
            { DataTypeEnum.Contact, new DataTableInfo("Contact", "ID") },
            { DataTypeEnum.Route, new DataTableInfo("Route", "RouteId") },
            { DataTypeEnum.Forms, new DataTableInfo("Forms", "ID") },
            { DataTypeEnum.Form, new DataTableInfo("Form", "Id") },
            { DataTypeEnum.GPSEvent, new DataTableInfo("VehicleEvent", "ID") },
            { DataTypeEnum.Report, new DataTableInfo("ExagoReport", "ID") },
            { DataTypeEnum.ReportLibrary, new DataTableInfo("ReportLibrary", "ID") },
            { DataTypeEnum.ScheduledReport, new DataTableInfo("ScheduledReport", "ID") },
            { DataTypeEnum.Session, new DataTableInfo("Session", "SessionID") },
            { DataTypeEnum.Dashboard, new DataTableInfo("Dashboard", "ID") },
            { DataTypeEnum.MergeDocument, new DataTableInfo("MergeDocument", "Id") },
            { DataTypeEnum.WorkOrder, new DataTableInfo("WorkOrder", "Id") },
            { DataTypeEnum.Equipment, new DataTableInfo("Equipment", "Id") },
            { DataTypeEnum.Service, new DataTableInfo("WorkTemplate", "Id") },
            { DataTypeEnum.Part, new DataTableInfo("Part", "Id") },
            { DataTypeEnum.Order, new DataTableInfo("Order", "Id") },
            { DataTypeEnum.PurchaseOrder, new DataTableInfo("PurchaseOrder", "Id") },
            { DataTypeEnum.Location, new DataTableInfo("Location", "Id") },
            { DataTypeEnum.MergeDocumentsSent, new DataTableInfo("MergeDocumentsSent", "Id") },
            { DataTypeEnum.ScheduledReportSent, new DataTableInfo("ScheduledReportSent", "Id") },
            { DataTypeEnum.SFVendor, new DataTableInfo("Organization", "Id") },
            { DataTypeEnum.Warranty, new DataTableInfo("Warranty", "Id") },
            { DataTypeEnum.ScheduleResource, new DataTableInfo("EquipmentWorkItem", "Id") },
            { DataTypeEnum.ScheduleTechnician, new DataTableInfo("Staff", "Id") },
            { DataTypeEnum.AccountCodeTransaction, new DataTableInfo("AccountCodeTransaction", "Id") },
        };

        public static string ToDataTypeString(this DataTypeEnum that)
        {
            switch (that)
            {
                case DataTypeEnum.ALL:
                    return DataTypeConst.ALL;
            }

            return that.ToString();
        }

        public static string ToDBViewName(this DataTypeEnum that)
        {
            switch (that)
            {
                case DataTypeEnum.AlternateSite:
                    return "qryGridAlt_Site";
                case DataTypeEnum.Contractor:
                    return "qryGridContractor";
                case DataTypeEnum.Contact:
                    return "qryGridContact";
                case DataTypeEnum.District:
                    return "qryGridDistrict";
                case DataTypeEnum.Document:
                    return "qryGridDocument";
                case DataTypeEnum.FieldTrip:
                    return "qryGridFieldTrip";
                case DataTypeEnum.GeoRegion:
                    return "qryGridGeoRegion";
                case DataTypeEnum.School:
                    return "qryGridSchool";
                case DataTypeEnum.ScheduleTechnician:
                case DataTypeEnum.Staff:
                    return "qryGridStaff";
                case DataTypeEnum.Student:
                    return "qryGridStudent";
                case DataTypeEnum.Trip:
                    return "qryGridTrip";
                case DataTypeEnum.TripStop:
                    return "qryGridTripStop";
                case DataTypeEnum.Vehicle:
                    return "qryGridVehicle";
                case DataTypeEnum.TripSchedule:
                    return "qryGridTripSchedule";
                case DataTypeEnum.TripStopSchedule:
                    return "qryGridTripStopSchedule";
                case DataTypeEnum.Route:
                    return "qryGridRoute";
                case DataTypeEnum.Forms:
                    return "qryGridForm";
                case DataTypeEnum.Form:
                    return "qryGridFormResult";
                case DataTypeEnum.GPSEvent:
                    return "qryGridVehicleEvent";
                case DataTypeEnum.Session:
                    return "qryGridSession";
                case DataTypeEnum.Dashboard:
                    return "qryGridDashboard";
                case DataTypeEnum.MergeDocument:
                    return "qryGridMergeDocument";
                case DataTypeEnum.Invoice:
                    return "qryGridInvoice";
                case DataTypeEnum.Customer:
                    return "qryGridCustomer";
                case DataTypeEnum.WorkOrder:
                    return "qryGridWorkOrder";
                case DataTypeEnum.Equipment:
                    return "qryGridEquipment";
                case DataTypeEnum.Service:
                    return "qryGridWorkTemplate";
                case DataTypeEnum.Quote:
                    return "qryGridQuote";
                case DataTypeEnum.Part:
                    return "qryGridPart";
                case DataTypeEnum.Order:
                    return "qryGridOrder";
                case DataTypeEnum.PurchaseOrder:
                    return "qryGridPurchaseOrder";
                case DataTypeEnum.Location:
                    return "qryGridLocation";
                case DataTypeEnum.MergeDocumentsSent:
                    return "qryGridMergeDocumentSent";
                case DataTypeEnum.ScheduledReportSent:
                    return "qryGridScheduledReportSent";
                case DataTypeEnum.SFVendor:
                    return "qryGridSFVendor";
                case DataTypeEnum.Warranty:
                    return "qryGridWarranty";
                case DataTypeEnum.Report:
                    return "qryGridExagoReport";
                case DataTypeEnum.ScheduleResource:
                    return "vwServiceToDo";
                case DataTypeEnum.AccountCodeTransaction:
                    return "qryGridAccountCodeTransaction";
            }

            return null;
        }

        public static string ToDBKey(this DataTypeEnum that)
        {
            return that.ToDataTableInfo()?.Key ?? string.Empty;
        }

        public static DataTableInfo ToDataTableInfo(this DataTypeEnum type)
        {
            return _dataTables.TryGetValue(type, out DataTableInfo result) ? result : null;
        }

        public static DataTypeEnum? Parse(string text)
        {
            return _typeValueMap.TryGetValue(text, out DataTypeEnum value) ? (DataTypeEnum?)value : null;
        }

        public static bool HasNoDbid(this DataTypeEnum type)
        {
            var noDbidList = new List<DataTypeEnum>()
            {
                DataTypeEnum.Vehicle,
                DataTypeEnum.Contractor,
                DataTypeEnum.District,
                DataTypeEnum.Staff,
                DataTypeEnum.Contact,
                DataTypeEnum.Document,
            };

            return noDbidList.Contains(type);
        }
    }

    public class DataTableInfo
    {
        public DataTableInfo(string table, string key)
        {
            Table = table;
            Key = key;
        }

        public string Table { get; set; }

        public string Key { get; set; }
    }
}
