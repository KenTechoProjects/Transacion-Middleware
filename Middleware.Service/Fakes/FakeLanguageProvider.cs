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
                    ["FBN002"] = "Échec de vérification du code PIN",
                    ["FBN003"] = "Profil dérivé",
                    ["FBN009"] = "Le client n'existe pas",
                    ["FBN012"] = "Réponse invalide",
                    ["FBN006"] = "Incompatibilité de périphérique",
                    ["FBN007"] = "Appareil désactivé",
                    ["FBN008"] = "Échec de validation d'entrée",
                    ["FBN011"] = "Paramètre d'entrée invalide",
                    ["FBN013"] = "Inadéquation des comptes et des clients",
                    ["FBN014"] = "Numéro de téléphone déjà utilisé",
                    ["FBN015"] = "Non concordance de numéro de compte",
                    ["FBN016"] = "Limite de transfert dépassée",
                    ["FBN031"] = "Bénéficiaire enregistré avec succès",
                    ["FBN032"] = "Impossible de sauver le bénéficiaire",
                    ["FBN037"] = "L'image n'a pas pu être traitée",

                    ["ONB001"] = "Paramètre de requête invalide",
                    ["ONB002"] = "OTP expiré",
                    ["ONB003"] = "Détails de carte invalide",
                    ["ONB004"] = "OTP non valide. Veuillez saisir un OTP correct",
                    ["ONB005"] = "Erreur de traitement du serveur",
                    ["ONB006"] = "Compte non trouvé",
                    ["ONB007"] = "Question de sécurité introuvable",
                    ["ONB008"] = "Client introuvable",
                    ["ONB009"] = "L'accès non autorisé",
                    ["ONB010"] = "Client déjà intégré",
                    ["ONB011"] = "Format de mot de passe invalide",
                    ["ONB012"] = "Demande de fonctionnalité Otp non valide",
                    ["ONB013"] = "Intégration du client non terminée",
                    ["ONB014"] = "Réponse de sécurité incorrecte",
                    ["ONB015"] = "Format de pin invalide",
                    ["ONB016"] = "Client déjà intégré avec un autre numéro de compte",
                    ["ONB017"] = "Votre pin ne peut pas être votre année de naissance",
                    ["ONB018"] = "Vous avez utilisé la broche précédemment.Veuillez utiliser une autre épingle",
                    ["ONB020"] = "Onboarding déjà initié",
                    ["ONB021"] = "Impossible de créer un nouveau compte",
                    ["ONB022"] = "Intégration non commencée",
                    ["ONB023"] = "Intégration défectueuse",
                    ["ONB024"] = "Le client a déjà un compte",

                    ["ONB025"] = "Format de pin invalide",
                    ["ONB026"] = "format de mot de passe invalide",
                    ["ONB029"] = "Numéro de téléphone déjà utilisé",

                    //FI Errors
                    ["FI22"] = "Compte non valide pour la transaction",
                    ["FI85"] = "Compte non valide pour la transaction",
                    ["FI07"] = "Compte invalide",
                    ["FI51"] = "Fonds insuffisants",
                    ["FI32"] = "Cross Currency non autorisé",
                    ["FI99"] = "Impossible de terminer la transaction",


                    //WalletMiddleWare Errors
                    ["WE01"] = "ID de portefeuille non valide",
                    ["WE03"] = "Paramètre d'entrée invalide",
                    ["WE04"] = "Le portefeuille existe déjà",
                    ["WE06"] = "ID de portefeuille source non valide",
                    ["WE07"] = "ID de portefeuille de destination non valide",
                    ["WE08"] = "Limite de portefeuille dépassée"


                }
            };
            return pack;
        }

    }
}