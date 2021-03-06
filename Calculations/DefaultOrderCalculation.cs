﻿using Biller.Core;
using Biller.Core.Articles;
using Biller.Core.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderTypes_Biller.Calculations
{
    public class DefaultOrderCalculation : Biller.Core.Utils.PropertyChangedHelper
    {
        private Order.Order _parentOrder;

        /// <summary>
        /// Default constructor for <see cref="DefaultOrderCalculation"/>.
        /// </summary>
        /// <param name="parentOrder">The calculations are based on the <see cref="Order"/> passed with the constructor.</param>
        public DefaultOrderCalculation(Order.Order parentOrder, bool calculate = false)
        {
            _parentOrder = parentOrder;
            ArticleSummary = new EMoney(0, true);
            NetArticleSummary = new EMoney(0, false);
            OrderSummary = new EMoney(0, true);
            NetOrderSummary = new EMoney(0, false);
            OrderRebate = new EMoney(0, true);
            NetShipment = new EMoney(0, false);
            NetOrderRebate = new EMoney(0, false);
            TaxValues = new ObservableCollection<Biller.Core.Models.TaxClassMoneyModel>();
            parentOrder.OrderedArticles.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnOrderedArticleCollectionChanged);
            parentOrder.OrderRebate.PropertyChanged += article_PropertyChanged;
            parentOrder.PaymentMethode.PropertyChanged += article_PropertyChanged;
            parentOrder.OrderShipment.PropertyChanged += article_PropertyChanged;
            parentOrder.PropertyChanged += parentOrder_PropertyChanged;

            if (calculate)
                CalculateValues();
        }

        void parentOrder_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PaymentMethode")
            {
                _parentOrder.PaymentMethode.PropertyChanged += article_PropertyChanged;
                CalculateValues();
            }
            if (e.PropertyName == "OrderShipment")
            {
                _parentOrder.OrderShipment.DefaultPrice.PropertyChanged += article_PropertyChanged;
                CalculateValues();
            }
        }

        void OnOrderedArticleCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                PropertyChangedHelper article = (PropertyChangedHelper)e.NewItems[0];
                article.PropertyChanged += article_PropertyChanged;
            }
            CalculateValues();
        }

        void article_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == "ArticleRecalculation")
                CalculateValues();
        }

        public virtual void CalculateValues()
        {
            TaxValues.Clear();
            ArticleSummary.Amount = 0;
            OrderedWeight = 0;
            // iterate through each article and add its value
            foreach (var article in _parentOrder.OrderedArticles)
            {
                ArticleSummary.Amount += article.RoundedGrossOrderValue.Amount;
                OrderedWeight += article.ArticleWeight * article.OrderedAmount;
                // taxclass listing
                if (TaxValues.Any(x => x.TaxClass.Name == article.TaxClass.Name))
                    TaxValues.Single(x => x.TaxClass.Name == article.TaxClass.Name).Value += article.ExactVAT;
                else
                    TaxValues.Add(new Biller.Core.Models.TaxClassMoneyModel() { Value = new Money(article.ExactVAT), TaxClass = article.TaxClass });
            }

            //NetArticleSummary
            NetArticleSummary.Amount = ArticleSummary.Amount;
            foreach (var item in TaxValues)
                NetArticleSummary.Amount -= item.Value.Amount;

            OrderSummary.Amount = ArticleSummary.Amount;
            NetOrderSummary.Amount = NetArticleSummary.Amount;
            
            // Discount
            if (_parentOrder.OrderRebate.Amount > 0)
            {
                var temp = OrderSummary.Amount;
                OrderSummary.Amount = OrderSummary.Amount * (1 - _parentOrder.OrderRebate.Amount);
                NetOrderRebate.Amount = NetOrderSummary.Amount - NetOrderSummary.Amount * (1 - _parentOrder.OrderRebate.Amount);
                NetOrderSummary.Amount = NetOrderSummary.Amount * (1 - _parentOrder.OrderRebate.Amount);
                OrderRebate = new EMoney(temp - OrderSummary.Amount);
                foreach (var item in TaxValues)               
                    item.Value = item.Value * (1 - _parentOrder.OrderRebate.Amount);
            }
            else
                OrderRebate = new EMoney(0);

            // Shipping
            if (!String.IsNullOrEmpty(_parentOrder.OrderShipment.Name))
            {
                // Germany: Supplementary needs to have the tax, that is used most on the invoice
                // Austria: Shipping has reduced taxes
                // CH: 
                OrderSummary.Amount += _parentOrder.OrderShipment.DefaultPrice.Amount;
                dynamic keyValueStore = Biller.UI.ViewModel.MainWindowViewModel.GetCurrentMainWindowViewModel().SettingsTabViewModel.KeyValueStore;
                var UseGermanSupplementaryTaxRegulation = keyValueStore.UseGermanSupplementaryTaxRegulation;
                if (UseGermanSupplementaryTaxRegulation == null)
                    UseGermanSupplementaryTaxRegulation = false;
                if (UseGermanSupplementaryTaxRegulation)
                {
                    var wholetax = 0.0;
                    var wholeShipmentTax = 0.0;
                    foreach (var item in TaxValues)
                        wholetax += item.Value.Amount;

                    List<Biller.Core.Models.TaxClassMoneyModel> temporaryTaxes = new List<Biller.Core.Models.TaxClassMoneyModel>();
                    var highestTaxClass = TaxValues.FirstOrDefault();
                    foreach (var taxitem in TaxValues)
                        if (taxitem.Value > highestTaxClass.Value)
                            highestTaxClass = taxitem;

                    var shipment = new OrderedArticle(new Article());
                    shipment.TaxClass = highestTaxClass.TaxClass;
                    shipment.OrderedAmount = 1;
                    shipment.OrderPrice.Price1 = _parentOrder.OrderShipment.DefaultPrice;

                    var TaxSupplementaryWorkSeparate = keyValueStore.TaxSupplementaryWorkSeperate;
                    if (TaxSupplementaryWorkSeparate == null)
                        TaxSupplementaryWorkSeparate = false;

                    if (TaxSupplementaryWorkSeparate)
                        temporaryTaxes.Add(new Biller.Core.Models.TaxClassMoneyModel() { Value = new Money(shipment.ExactVAT), TaxClass = highestTaxClass.TaxClass, TaxClassAddition = " auf Nebenleistungen" });
                    else
                        TaxValues.Where(x=>x.TaxClass.Equals(highestTaxClass.TaxClass)).First().Value += (shipment.ExactVAT);
                    wholeShipmentTax +=  shipment.ExactVAT;

                    NetShipment.Amount = _parentOrder.OrderShipment.DefaultPrice.Amount - wholeShipmentTax;
                    NetOrderSummary.Amount += NetShipment.Amount;

                    foreach (Biller.Core.Models.TaxClassMoneyModel temporaryTax in temporaryTaxes)
                        TaxValues.Add(temporaryTax);
                }
                else
                {
                    var shipment = new OrderedArticle(new Article());
                    try
                    {
                        shipment.TaxClass = (TaxClass)keyValueStore.ShipmentTaxClass;
                        shipment.OrderedAmount = 1;
                        shipment.OrderPrice.Price1 = _parentOrder.OrderShipment.DefaultPrice;
                    }
                    catch
                    {
                        Biller.UI.ViewModel.MainWindowViewModel.GetCurrentMainWindowViewModel().NotificationManager.ShowNotification("Fehler bei Berechnung des Betrages", "Es wurde keine Steuerklasse für Nebenleistungen angegeben. Überprüfen Sie die Einstellungen.");
                        return;
                    }
                    TaxValues.Add(new Biller.Core.Models.TaxClassMoneyModel() { Value = new Money(shipment.ExactVAT), TaxClass = shipment.TaxClass, TaxClassAddition = " auf Nebenleistungen" });
                    NetShipment.Amount = _parentOrder.OrderShipment.DefaultPrice.Amount - shipment.ExactVAT;
                    NetOrderSummary.Amount += NetShipment.Amount;
                }
            }

            // Skonto
            if (_parentOrder.PaymentMethode.Discount.Amount > 0)
                CashBack = new EMoney(_parentOrder.PaymentMethode.Discount.Amount * ArticleSummary.Amount);
            else
                CashBack = new EMoney(0);
            
            RaiseUpdateManually("ArticleSummary");
            RaiseUpdateManually("TaxValues");
            RaiseUpdateManually("OrderRebate");
            RaiseUpdateManually("CashBack");
            RaiseUpdateManually("NetArticleSummary");
            RaiseUpdateManually("NetOrderSummary");
            RaiseUpdateManually("OrderSummary");
            RaiseUpdateManually("NetShipment");
            RaiseUpdateManually("NetOrderRebate");
        }

        /// <summary>
        /// With taxes
        /// </summary>
        public EMoney ArticleSummary { get { return GetValue(() => ArticleSummary); } set { SetValue(value); } }

        public EMoney NetArticleSummary { get { return GetValue(() => NetArticleSummary); } set { SetValue(value); } }

        public EMoney OrderSummary { get { return GetValue(() => OrderSummary); } set { SetValue(value); } }

        public EMoney NetOrderSummary { get { return GetValue(() => NetOrderSummary); } set { SetValue(value); } }

        public EMoney NetShipment { get { return GetValue(() => NetShipment); } set { SetValue(value); } }

        /// <summary>
        /// Skonto
        /// </summary>
        public EMoney CashBack { get { return GetValue(() => CashBack); } set { SetValue(value); } }

        public EMoney OrderRebate { get { return GetValue(() => OrderRebate); } set { SetValue(value); } }

        public EMoney NetOrderRebate { get { return GetValue(() => NetOrderRebate); } set { SetValue(value); } }

        public ObservableCollection<Biller.Core.Models.TaxClassMoneyModel> TaxValues { get { return GetValue(() => TaxValues); } set { SetValue(value); } }

        public double OrderedWeight { get { return GetValue(() => OrderedWeight); } set { SetValue(value); } }

    }
}