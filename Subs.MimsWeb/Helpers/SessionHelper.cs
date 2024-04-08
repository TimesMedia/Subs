using System;
using System.Web;
using System.Web.SessionState;
using Subs.MimsWeb.Models;
using Subs.Data;
using System.Collections.Generic;

namespace Subs.MimsWeb
{
    public enum SessionKey
    {
        LoginRequest,
        WebProducts,
        Basket,
        PayUReference,
        PrimeBasket
    }

    public class BasketOption
    {
        //private List<int> gProducts = new List<int>();

        //public int this[int index] 
        //{
        //    get 
        //    {
        //        return gProducts[index];
        //    }
        //}

        public bool MIC = false;
        

        public bool HasOptions
        {
            get 
            {
                if (MIC == false)
                {
                    return false;
                }
                else
                { 
                    return true;
                }
            }
        }


        public void Clear()
        {
            //gProducts.Clear();
            MIC = false;
        }
    }


    public static class SessionHelper { 
    public static void Set(HttpSessionStateBase session, SessionKey key, object value) { 
            session[Enum.GetName(typeof(SessionKey), key)] = value;
    }

    public static T Get<T>(HttpSessionStateBase session, SessionKey key) { 
        object dataValue = session[Enum.GetName(typeof(SessionKey), key)];

        if (dataValue != null && dataValue is T)
        {
            return (T)dataValue;
        }
        else 
        {
            return default(T);
        }
    }


    public static LoginRequest GetLoginRequest(HttpSessionStateBase session)
    {
        LoginRequest lLoginRequest = Get<LoginRequest>(session, SessionKey.LoginRequest);
        if (lLoginRequest == null)
        {
            lLoginRequest = new LoginRequest();
            Set(session, SessionKey.LoginRequest, lLoginRequest);
        }
        return lLoginRequest;
    }

    public static WebProducts GetWebProducts (HttpSessionStateBase session)
    {
        WebProducts lWebProducts = Get<WebProducts>(session, SessionKey.WebProducts);
        if (lWebProducts == null)
        {
            lWebProducts = new WebProducts();

            Set(session, SessionKey.WebProducts, lWebProducts);
        }
        return lWebProducts;
    }

    public static Basket GetBasket(HttpSessionStateBase session)
    {
        Basket lBasket = Get<Basket>(session, SessionKey.Basket);
        if (lBasket == null)
        {
            lBasket = new Basket();
            Set(session, SessionKey.Basket, lBasket);
        }
        return lBasket;
    }

    public static BasketOption GetBasketOption(HttpSessionStateBase session)
    {
        BasketOption lBasketOption = Get<BasketOption>(session, SessionKey.PrimeBasket);
        if (lBasketOption == null)
        {
            lBasketOption = new BasketOption();
            Set(session, SessionKey.PrimeBasket, lBasketOption);
        }
        return lBasketOption;
    }

        public static string GetPayUReference(HttpSessionStateBase session)
    {
        string lPayUReference= Get<string>(session, SessionKey.PayUReference);
        if (String.IsNullOrWhiteSpace(lPayUReference))
        {
            Set(session, SessionKey.PayUReference, lPayUReference);
        }
        return lPayUReference;
    }


    }
}