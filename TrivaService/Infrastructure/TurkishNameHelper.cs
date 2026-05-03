namespace TrivaService.Infrastructure
{
    public static class TurkishNameHelper
    {
        private static readonly Dictionary<string, string> EntityNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Customer"]       = "Müşteri",
            ["Service"]        = "Servis",
            ["ServiceItem"]    = "Servis Kalemi",
            ["ServiceVisuals"] = "Servis Görseli",
            ["Item"]           = "Ürün",
            ["Supplier"]       = "Tedarikçi",
            ["Users"]          = "Kullanıcı",
            ["Roles"]          = "Rol",
            ["History"]        = "Geçmiş",
        };

        private static readonly Dictionary<string, string> PropertyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"]                   = "No",
            ["CreateDate"]           = "Oluşturma Tarihi",
            ["UpdateDate"]           = "Güncelleme Tarihi",
            ["IsActive"]             = "Aktif",
            // Customer
            ["CustomerName"]         = "Müşteri Adı",
            ["CustomerPhone"]        = "Telefon",
            ["CustomerAddress"]      = "Adres",
            ["Notes"]                = "Notlar",
            ["LastServiceDate"]      = "Son Servis Tarihi",
            ["TotalServiceCount"]    = "Toplam Servis Sayısı",
            // Service
            ["CustomerId"]           = "Müşteri No",
            ["ServiceCode"]          = "Servis Kodu",
            ["FaultDescription"]     = "Arıza Açıklaması",
            ["ServiceDescription"]   = "Servis Açıklaması",
            ["ServiceNotes"]         = "Servis Notları",
            ["ServiceAddress"]       = "Servis Adresi",
            ["ReceivedDate"]         = "Alınma Tarihi",
            ["CompletedDate"]        = "Tamamlanma Tarihi",
            ["DeliveredDate"]        = "Teslim Tarihi",
            ["Status"]               = "Durum",
            ["EstimatedCost"]        = "Tahmini Maliyet",
            ["FinalCost"]            = "Toplam Maliyet",
            ["IsPaymentComplete"]    = "Ödeme Tamamlandı",
            // ServiceItem
            ["ServiceId"]            = "Servis No",
            ["ItemId"]               = "Ürün No",
            ["Quantity"]             = "Miktar",
            ["UnitPrice"]            = "Birim Fiyat",
            ["UnitCost"]             = "Birim Maliyet",
            ["TotalPrice"]           = "Toplam Fiyat",
            // ServiceVisuals
            ["ServiceVisualName"]    = "Görsel Adı",
            ["ServiceDocumentUrl"]   = "Belge URL",
            // Item
            ["ItemName"]             = "Ürün Adı",
            ["ItemCode"]             = "Ürün Kodu",
            ["ItemBrand"]            = "Marka",
            ["ItemModel"]            = "Model",
            ["ItemBarcode"]          = "Barkod",
            ["ItemDescription"]      = "Ürün Açıklaması",
            ["ItemType"]             = "Ürün Türü",
            ["ItemQuantity"]         = "Stok Miktarı",
            ["ItemPrice"]            = "Fiyat",
            ["SupplierId"]           = "Tedarikçi No",
            // Supplier
            ["SupplierName"]         = "Tedarikçi Adı",
            ["SupplierPhone"]        = "Telefon",
            ["SupplierContactPerson"]= "Yetkili Kişi",
            ["SupplierEmail"]        = "E-posta",
            ["SupplierAddress"]      = "Adres",
            // Users
            ["UserName"]             = "Kullanıcı Adı",
            ["UserPasswordHash"]     = "Şifre",
            ["UserPhone"]            = "Telefon",
            ["UserNotes"]            = "Notlar",
            ["RolesId"]              = "Rol No",
            // Roles
            ["RoleName"]             = "Rol Adı",
            ["RoleDescription"]      = "Açıklama",
        };

        public static string GetEntityDisplayName(string entityName) =>
            EntityNames.TryGetValue(entityName, out var display) ? display : entityName;

        public static string GetPropertyDisplayName(string propertyName) =>
            PropertyNames.TryGetValue(propertyName, out var display) ? display : propertyName;
    }
}
