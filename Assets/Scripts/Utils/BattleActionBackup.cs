using System.Collections.Generic;
using UnityEngine;

// Sistema de backup e restauração de valores de BattleActions
public class BattleActionBackup
{
    [System.Serializable]
    private class EffectBackup
    {
        public int power;
        public int statusPower;
        public int statusDuration;
        public int selfEffectPower;
        public int selfStatusPower;
        public int selfStatusDuration;
    }
    
    [System.Serializable]
    private class ActionBackup
    {
        public BattleAction action;
        public int manaCost;
        public List<EffectBackup> effects = new List<EffectBackup>();
    }
    
    private List<ActionBackup> backups = new List<ActionBackup>();
    
    // Salva valores atuais de uma lista
    public void SaveActions(List<BattleAction> actions)
    {
        backups.Clear();
        
        if (actions == null) return;
        
        foreach (var action in actions)
        {
            if (action == null) continue;
            
            ActionBackup backup = new ActionBackup
            {
                action = action,
                manaCost = action.manaCost
            };
            
            if (action.effects != null)
            {
                foreach (var effect in action.effects)
                {
                    backup.effects.Add(new EffectBackup
                    {
                        power = effect.power,
                        statusPower = effect.statusPower,
                        statusDuration = effect.statusDuration,
                        selfEffectPower = effect.selfEffectPower,
                        selfStatusPower = effect.selfStatusPower,
                        selfStatusDuration = effect.selfStatusDuration
                    });
                }
            }
            
            backups.Add(backup);
        }
    }
    
    // Restaura valores salvos
    public void RestoreActions()
    {
        foreach (var backup in backups)
        {
            if (backup.action == null) continue;
            
            backup.action.manaCost = backup.manaCost;
            
            if (backup.action.effects != null && backup.effects != null)
            {
                int count = Mathf.Min(backup.action.effects.Count, backup.effects.Count);
                
                for (int i = 0; i < count; i++)
                {
                    var effect = backup.action.effects[i];
                    var savedEffect = backup.effects[i];
                    
                    effect.power = savedEffect.power;
                    effect.statusPower = savedEffect.statusPower;
                    effect.statusDuration = savedEffect.statusDuration;
                    effect.selfEffectPower = savedEffect.selfEffectPower;
                    effect.selfStatusPower = savedEffect.selfStatusPower;
                    effect.selfStatusDuration = savedEffect.selfStatusDuration;
                }
            }
        }
        
        backups.Clear();
    }
}