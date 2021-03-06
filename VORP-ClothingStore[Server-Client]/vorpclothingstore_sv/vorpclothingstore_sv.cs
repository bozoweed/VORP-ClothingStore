﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace vorpclothingstore_sv
{
    public class vorpclothingstore_sv : BaseScript
    {
        //            vorpcharacter:setPlayerCompChange
        public vorpclothingstore_sv()
        {
            EventHandlers["vorpclothingstore:getPlayerCloths"] += new Action<Player>(getPlayerCloths);
            EventHandlers["vorpclothingstore:buyPlayerCloths"] += new Action<Player, double, string, bool, string>(buyPlayerCloths);
            EventHandlers["vorpclothingstore:setOutfit"] += new Action<Player, string>(LoadOutfit);
        }

        private void LoadOutfit([FromSource]Player source,  string json)
        {
            TriggerEvent("vorpcharacter:setPlayerCompChange", int.Parse(source.Handle), json);
        }

        private void buyPlayerCloths([FromSource]Player source, double totalCost, string jsonCloths, bool saveOut, string nameOut)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

            TriggerEvent("vorp:getCharacter", _source, new Action<dynamic>((user) =>
            {
                double money = user.money;

                if (totalCost <= money)
                {
                    TriggerEvent("vorp:removeMoney", _source, 0, totalCost);

                    Exports["ghmattimysql"].execute($"UPDATE characters SET compPlayer = ? WHERE identifier=?", new[] { jsonCloths, sid });

                    if (saveOut)
                    {
                        Exports["ghmattimysql"].execute($"INSERT INTO outfits (identifier,title,comps) VALUES (?,?,?)", new[] { sid, nameOut, jsonCloths });
                    }

                    source.TriggerEvent($"vorpclothingstore:startBuyCloths", true);
                    source.TriggerEvent("vorp:Tip", LoadConfig.Langs["SuccessfulBuy"] + $" ${totalCost}", 4000);
                }
                else
                {
                    source.TriggerEvent("vorp:Tip", LoadConfig.Langs["NoMoney"], 4000);
                    source.TriggerEvent($"vorpclothingstore:startBuyCloths", false);
                }

            }));
        }

        private void getPlayerCloths([FromSource]Player source)
        {
            int _source = int.Parse(source.Handle);

            TriggerEvent("vorpcharacter:getPlayerComps", _source, new Action<dynamic>((cb) =>
            {

                source.TriggerEvent($"{API.GetCurrentResourceName()}:LoadYourCloths", cb.cloths, cb.skins);

            }));

            string sid = "steam:" + source.Identifiers["steam"];

            Exports["ghmattimysql"].execute("SELECT * FROM outfits WHERE identifier = ?", new[] { sid }, new Action<dynamic>((result) =>
            {
                if (result.Count == 0)
                {
                    Debug.WriteLine("User not have outfits");
                }
                else
                {
                    source.TriggerEvent($"{API.GetCurrentResourceName()}:LoadYourOutfits", result);
                }

            }));

        }
    }
}
