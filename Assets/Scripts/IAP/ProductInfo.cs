using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProductInfo
{
    public string subscriptionNumberOfUnits;
    public string subscriptionPeriodUnit = "";
    public string localizedPrice;
    public string isoCurrencyCode;
    public string localizedPriceString;
    public string localizedTitle;
    public string localizedDescription;
    public string introductoryPriceLocale;
    public string introductoryPriceNumberOfPeriods;
    public string numberOfUnits;
    public string unit;

    // I have only tested 2 = month, I have not tested other values in this list
    public Dictionary<string, string> SubscriptionPeriods = new Dictionary<string, string>()
    {
        {"0", "day" },
        {"1", "week" },
        {"2", "month" },
        {"3", "year" },
        {"4", "not available" }
    };

    public static ProductInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<ProductInfo>(jsonString);
    }

    public string GetSubscriptionPeriod()
    {
        if (this.subscriptionPeriodUnit == "") { return ""; };
        return SubscriptionPeriods[this.subscriptionPeriodUnit];

    }

    //IAP Product details productJSON {"subscriptionNumberOfUnits":"2","subscriptionPeriodUnit":"2","localizedPrice":"0.99","isoCurrencyCode":"USD","localizedPriceString":"$0.99","localizedTitle":"IAP Title","localizedDescription":"Enjoy the subscription!","introductoryPrice":"","introductoryPriceLocale":"","introductoryPriceNumberOfPeriods":"","numberOfUnits":"","unit":""}

}