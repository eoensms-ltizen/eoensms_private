using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using stackRPG;

public static class MAIUser
{
    public static IEnumerator Progress(AIUser aiUser, MUser user)
    {
        int totalGold = user.m_gold;        
        Dictionary <UserAction, int> canUseGoldByAction;
        GetGoldByAction(aiUser, totalGold, out canUseGoldByAction);

        //! 순서대로 연다.
        int openGold = canUseGoldByAction[UserAction.Open];        
        while (UseGoldToOpen(user, ref openGold)) yield return null;

        //! 렌덤으로 만듬
        int makeGold = canUseGoldByAction[UserAction.Make];        
        while (UseGoldToMake(user, ref makeGold)) yield return null;

        //! 가지고있는것중 랜덤으로 업글
        int upgradeGold = canUseGoldByAction[UserAction.Upgrade];        
        while (UserGoldToUpgrade(user, ref upgradeGold)) yield return null;

        //! 남은돈처리
        totalGold = openGold + makeGold + upgradeGold;        
        switch (aiUser.m_lastAction)
        {
            case LastUserAction.Make:
                while (UseGoldToMake(user, ref totalGold)) yield return null;
                break;
            case LastUserAction.Save:
                
                break;
            case LastUserAction.Upgrade:
                while (UserGoldToUpgrade(user, ref totalGold)) yield return null;
                break;
        }
    }

    static bool UseGoldToOpen(MUser user,ref int gold)
    {
        foreach (UnitLevelTable unitLevelTable in user.m_haveUnit)
        {
            if (unitLevelTable.m_isOpend == false)
            {
                int id = unitLevelTable.m_id;
                Unit unit = MUnitManager.Instance.GetUnit(id);
                int needGold = unit.m_openPrice;
                if (gold > needGold)
                {
                    gold -= needGold;
                    MGameManager.Instance.OpenUnit(unit);
                    return true;
                }
            }
        }
        return false;
    }

    static bool UseGoldToMake(MUser user, ref int gold)
    {
        List<Unit> canMakeUnits;
        GetCanMakeUnit(user, gold, out canMakeUnits);
        if (canMakeUnits.Count == 0) return false;

        Unit ranUnit = canMakeUnits[MSettings.Random(0, canMakeUnits.Count)];
        MGameManager.Instance.MakeUnit(ranUnit);
        gold -= ranUnit.m_makePrice;
        return true;
    }

    static bool UserGoldToUpgrade(MUser user, ref int gold)
    {
        List<Unit> canUpgradeUnit;
        GetCanUpgradeUnit(user, gold, out canUpgradeUnit);
        if (canUpgradeUnit.Count == 0) return false;

        Unit ranUnit = canUpgradeUnit[MSettings.Random(0, canUpgradeUnit.Count)];
        int level = user.GetUnitLevelTable(ranUnit.m_id).m_level;
        gold -= ranUnit.m_upgradeCost[level];
        MGameManager.Instance.UpgradeUnit(ranUnit);
        return true;
    }

    private static void GetCanMakeUnit(MUser user, int gold, out List<Unit> canMakeUnits)
    {
        canMakeUnits = new List<Unit>();
        foreach (UnitLevelTable unitLevelTable in user.m_haveUnit)
        {
            Unit unit = MUnitManager.Instance.GetUnit(unitLevelTable.m_id);
            if (gold >= unit.m_makePrice) canMakeUnits.Add(unit);
        }
    }

    public static void GetGoldByAction(AIUser aiUser, int totalGold, out Dictionary<UserAction, int> canUseGoldByAction)
    {
        canUseGoldByAction = new Dictionary<UserAction, int>();
        float totalValue = aiUser.m_useGold_make + aiUser.m_useGold_opend + aiUser.m_useGold_upgrade;
        canUseGoldByAction.Add(UserAction.Open, (int)(aiUser.m_useGold_opend / totalValue * totalGold));
        canUseGoldByAction.Add(UserAction.Make, (int)(aiUser.m_useGold_make / totalValue * totalGold));
        canUseGoldByAction.Add(UserAction.Upgrade, (int)(aiUser.m_useGold_upgrade / totalValue * totalGold));
    }

    public static void GetCanUpgradeUnit(MUser user, int gold, out List<Unit> canUpgradeUnit)
    {
        canUpgradeUnit = new List<Unit>();
        foreach (UnitLevelTable unitLevelTable in user.m_haveUnit)
        {
            if (unitLevelTable.m_isOpend == true)
            {
                int level = unitLevelTable.m_level;
                Unit unit = MUnitManager.Instance.GetUnit(unitLevelTable.m_id);
                if (gold >= unit.m_upgradeCost[level]) canUpgradeUnit.Add(unit);
            }
        }
    }
}

