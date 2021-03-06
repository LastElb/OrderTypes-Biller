﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Biller.Core.Utils;
using Biller.Core.Document;

namespace OrderTypes_Biller.Docket
{
    public class DocketParser : Biller.Core.Interfaces.DocumentParser
    {
        /// <summary>
        /// Parses <see cref="Customer"/> and <see cref="OrderedArticles"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <param name="data"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public bool ParseAdditionalData(ref Document document, XElement data, Biller.Core.Interfaces.IDatabase database)
        {
            if (document is Docket)
            {
                var result = database.GetCustomer(data.Element("CustomerID").Value);
                var customer = result.Result;
                (document as Docket).Customer = customer;

                var articles = data.Element("OrderedArticles").Elements();
                (document as Docket).OrderedArticles.Clear();
                foreach (XElement article in articles)
                {
                    var temp = new Biller.Core.Articles.OrderedArticle();

                    var task = database.ArticleUnits();
                    temp.ArticleUnit = task.Result.Where(x => x.Name == article.Element("ArticleUnit").Value).Single();

                    var taskTaxClass = database.TaxClasses();
                    temp.TaxClass = taskTaxClass.Result.Where(x => x.Name == article.Element("TaxClass").Value).Single();

                    temp.ParseFromXElement(article);
                    (document as Docket).OrderedArticles.Add(temp);
                }

                try
                {
                    (document as Docket).PaymentMethode.ParseFromXElement(data.Element((document as Docket).PaymentMethode.XElementName));
                    (document as Docket).OrderShipment.ParseFromXElement(data.Element((document as Docket).OrderShipment.XElementName));
                }
                catch (Exception e)
                { }
                try
                {
                    (document as Docket).SmallBusiness = Boolean.Parse(data.Element("SmallBusiness").Value);
                    if ((document as Docket).SmallBusiness)
                        (document as Docket).OrderCalculation = new Calculations.SmallBusinessCalculation(document as Order.Order, true);
                    else if (!((document as Docket).OrderCalculation is Calculations.DefaultOrderCalculation))
                        (document as Docket).OrderCalculation = new Calculations.DefaultOrderCalculation(document as Order.Order, true);
                }
                catch { }

                var money = new Money(0);
            }
            else
            {
                return false;
            }
            return true;
        }

        public string DocumentType { get { return "Docket"; } }

        public bool ParseAdditionalPreviewData(ref dynamic document, XElement data)
        {
            var money = new Biller.Core.Utils.EMoney(0);
            money.ParseFromXElement(data.Element("PreviewValue").Element(money.XElementName));
            document.Value = money;
            document.LocalizedDocumentType = LocalizedDocumentType;
            return true;
        }

        public string LocalizedDocumentType
        {
            get
            {
                var invoice = new Docket();
                return invoice.LocalizedDocumentType;
            }
        }
    }
}
