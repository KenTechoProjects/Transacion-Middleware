using System;
using System.Collections.Generic;
using Middleware.Service.Utilities;

namespace Middleware.Service.Fakes
{
    public class FakeLanguageProvider : ILanguageConfigurationProvider
    {
        readonly IDictionary<string, LanguagePack> _packs;
        public LanguagePack GetPack(string language)
        {
            return (_packs.TryGetValue(language, out var pack)) ? pack : null;
        }

        public FakeLanguageProvider()
        {
            _packs = new Dictionary<string, LanguagePack>(2)
            {
                { "en", GetEnglishPack() },
                { "fr", GetFrenchPack() }
            };
        }


        private LanguagePack GetEnglishPack()
        {
            var pack = new LanguagePack("An error occurred")
            {
                Mappings = new Dictionary<string, string>
                {
                    ["FBN001"] = "Invalid Username or Password",
                    ["FBN002"] = "Pin verification failure",
                    ["FBN003"] = "Profile deactivated",
                    ["FBN009"] = "Invalid answer",
                    ["FBN012"] = "Profile not found",
                    ["FBN006"] = "Device mismatch",
                    ["FBN007"] = "Device disabled",
                    ["FBN008"] = "Input validation failure",
                    ["FBN011"] = "Invalid input parameter",
                    ["FBN013"] = "Confirm account number and branch",
                    ["FBN014"] = "Phone Number already used",
                    ["FBN015"] = "Account number mismatch",
                    ["FBN016"] = "Transfer Limit Exceeded",
                    ["FBN017"] = "The specified item could not be found",
                    ["FBN019"] = "Wallet already exists",
                    ["FBN023"] = "The request does not belong to the specified user",
                    ["FBN024"] = "Request not yet complete",                 
                   
                    ["FBN026"] = "Transaction reference is missing",
                    ["FBN027"] = "Duplicate transaction detected",
                    ["FBN028"] = "Device is currently assigned to another customer",
                    ["FBN029"] = "New device. Please register it",
                    ["FBN030"] = "Unrecognised device. You have already reached the limit allowed",
                    ["FBN031"] = "Beneficiary saved successfully",
                    ["FBN032"] = "Unable to save beneficiary",
                    ["FBN033"] = "Incorrect code or code has expired",
                    ["FBN034"] = "Invalid Operation",
                    ["FBN035"] = "Device not found",
                    ["FBN037"] = "Image could not be processed",
                    ["FBN038"] = "Invalid Beneficiary account",
                    ["FBN047"] = "Profile not found",
                    ["FBN048"] = "Wallet opening not successfully created try again!",
                    ["FBN050"] = "Image Profile Not Created",
                    ["FBN051"] = "Please provide the user photo",
                    ["FBN052"] = "Please try resuming onboarding to complete your onboarding process",
                    ["FBN053"] = "Beneficiary already exists",
                    ["FBN056"] = "Daily transaction limit exceeded",
                    ["FBN057"] = "Single transfer Limit Exceeded",
                    ["ONB001"] = "Invalid request parameter",
                    ["ONB002"] = "OTP Expired",
                    ["ONB003"] = "Invalid card details",
                    ["ONB004"] = "Invalid OTP. Please enter correct OTP",
                    ["ONB005"] = "Server processing error",
                    ["ONB006"] = "Unable to retrieve account name. Please check account number and try again or contact beneficiary to reconfirm details.",
                    ["ONB007"] = "Unable to retrieve security questions",
                    ["ONB008"] = "Customer not found",
                    ["ONB009"] = "Unauthorized access",
                    ["ONB010"] = "Customer already onboarded ",
                    ["ONB011"] = "Invalid password",
                    ["ONB012"] = "Invalid Otp feature request",
                    ["ONB013"] = "Customer onboarding not completed",
                    ["ONB014"] = "Wrong answer",
                    ["ONB015"] = "Invalid PIN",
                    ["ONB016"] = "Mobile number matched with enrolled customer account number",
                    ["ONB017"] = "Your PIN cannot be your birthyear",
                    ["ONB018"] = "You have used this PIN previously. Kindly use another PIN",
                    ["ONB020"] = "Onboarding already initiated",
                    ["ONB021"] = "Unable to create new account",
                    ["ONB022"] = "Onboarding not started",
                    ["ONB023"] = "Onboarding defective",
                    ["ONB024"] = "Customer already has account",
                    ["ONB025"] = "Invalid pin format",
                    ["ONB026"] = "Invalid password format",
                    ["ONB029"] = "Phone Number already used",

                    //FI Errors
                    ["FI22"] = "Account not valid for transaction",
                    ["FI85"] = "Account not valid for transaction",
                    ["FI07"] = "Invalid account",
                    ["FI51"] = "Insufficient fund",
                    ["FI32"] = "Cross Currency transactions not allowed",
                    ["FI99"] = "Unable to complete transaction",


                    //WalletMiddleWare Errors
                    ["W"] = "Please Provide The Narration",
                    ["WE01"] = "Invalid wallet ID",
                    ["WE03"] = "Invalid input parameter",
                    ["WE04"] = "Wallet already exists",
                    ["WE04"] = "Wallet already exists",
                    ["WE05"] = "Please Provide The Narration",
                    ["WE07"] = "Invalid destination wallet ID",
                    ["WE08"] = "Wallet limit exceeded",
                    ["W07"] = "Insufficient funds",
                    ["W95"] = "Narration is required to complete transaction",
                    ["W78"] = "Wallet not created"

                }
            };
            return pack;
        }

        private LanguagePack GetFrenchPack()
        {
            var pack = new LanguagePack("Une erreur est survenue")
            {
                Mappings = new Dictionary<string, string>
                {
                    ["FBN001"] = "Nom d'utilisateur ou mot de passe invalide",
                    ["FBN002"] = "�chec de v�rification du code PIN",
                    ["FBN003"] = "Profil d�riv�",
                    ["FBN009"] = "Le client n'existe pas",
                    ["FBN012"] = "R�ponse invalide",
                    ["FBN006"] = "Incompatibilit� de p�riph�rique",
                    ["FBN007"] = "Appareil d�sactiv�",
                    ["FBN008"] = "�chec de validation d'entr�e",
                    ["FBN011"] = "Param�tre d'entr�e invalide",
                    ["FBN013"] = "Inad�quation des comptes et des clients",
                    ["FBN014"] = "Num�ro de t�l�phone d�j� utilis�",
                    ["FBN015"] = "Non concordance de num�ro de compte",
                    ["FBN016"] = "Limite de transfert d�pass�e",
                    ["FBN031"] = "B�n�ficiaire enregistr� avec succ�s",
                    ["FBN032"] = "Impossible de sauver le b�n�ficiaire",
                    ["FBN037"] = "L'image n'a pas pu �tre trait�e",

                    ["ONB001"] = "Param�tre de requ�te invalide",
                    ["ONB002"] = "OTP expir�",
                    ["ONB003"] = "D�tails de carte invalide",
                    ["ONB004"] = "OTP non valide. Veuillez saisir un OTP correct",
                    ["ONB005"] = "Erreur de traitement du serveur",
                    ["ONB006"] = "Compte non trouv�",
                    ["ONB007"] = "Question de s�curit� introuvable",
                    ["ONB008"] = "Client introuvable",
                    ["ONB009"] = "L'acc�s non autoris�",
                    ["ONB010"] = "Client d�j� int�gr�",
                    ["ONB011"] = "Format de mot de passe invalide",
                    ["ONB012"] = "Demande de fonctionnalit� Otp non valide",
                    ["ONB013"] = "Int�gration du client non termin�e",
                    ["ONB014"] = "R�ponse de s�curit� incorrecte",
                    ["ONB015"] = "Format de pin invalide",
                    ["ONB016"] = "Client d�j� int�gr� avec un autre num�ro de compte",
                    ["ONB017"] = "Votre pin ne peut pas �tre votre ann�e de naissance",
                    ["ONB018"] = "Vous avez utilis� la broche pr�c�demment.Veuillez utiliser une autre �pingle",
                    ["ONB020"] = "Onboarding d�j� initi�",
                    ["ONB021"] = "Impossible de cr�er un nouveau compte",
                    ["ONB022"] = "Int�gration non commenc�e",
                    ["ONB023"] = "Int�gration d�fectueuse",
                    ["ONB024"] = "Le client a d�j� un compte",

                    ["ONB025"] = "Format de pin invalide",
                    ["ONB026"] = "format de mot de passe invalide",
                    ["ONB029"] = "Num�ro de t�l�phone d�j� utilis�",

                    //FI Errors
                    ["FI22"] = "Compte non valide pour la transaction",
                    ["FI85"] = "Compte non valide pour la transaction",
                    ["FI07"] = "Compte invalide",
                    ["FI51"] = "Fonds insuffisants",
                    ["FI32"] = "Cross Currency non autoris�",
                    ["FI99"] = "Impossible de terminer la transaction",


                    //WalletMiddleWare Errors
                    ["WE01"] = "ID de portefeuille non valide",
                    ["WE03"] = "Param�tre d'entr�e invalide",
                    ["WE04"] = "Le portefeuille existe d�j�",
                    ["WE06"] = "ID de portefeuille source non valide",
                    ["WE07"] = "ID de portefeuille de destination non valide",
                    ["WE08"] = "Limite de portefeuille d�pass�e"


                }
            };
            return pack;
        }

    }
}