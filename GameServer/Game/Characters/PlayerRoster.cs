using Proto;

namespace MollyPS.GameServer.Game.Characters;

public static class PlayerRoster
{
    public static void Populate(PlayerInstance player, uint potential = 0)
    {
         
        foreach (var template in DataTable.PlayableChars.OrderBy(id => id, StringComparer.Ordinal))
        {
            if (DataTable.CharToNum(template) == 0) continue;

            var id = (ulong)player.Chars.Count + 1;
            var (hp, sp) = DataTable.GetVitals(template);
            var character = new CHAR_INFO
            {
                Objid = id,
                Templateid = template,
                CharType = CHAR_TYPE.DefaultType,
                Level = 1,
                PotentialLevel = potential,
                BattleInfo = new BATTLE_INFO { Hp = hp, Ultimatesp = (float)sp },
                Talent = new TALENT(),
                TrialData = new TRIAL_CHAR_DATA(),
                BattleMgrInfo = new BATTLE_MGR_INFO
                {
                    MsgGeneration = (uint)id,
                    BattleInstId = (uint)id,
                    PartInstInfo = new BATTLE_PART_INST_INFO()
                }
            };
            PopulateSkills(character, template);
            player.Chars.Add(character);
            player.ObjIdByTemplate[template] = id;
        }
    }

    private static void PopulateSkills(CHAR_INFO character, string template)
    {
        if (DataTable.GetSkills(template) is not { } skills) return;

        character.NormalSkill = skills.normal;
        character.SkillInfo = new SKILL_INFO
        {
            NormalSkill = skills.normal,
            ComboSkill = skills.combo,
            UltimateSkill = skills.ultimate,
            DispNormalAttackSkill = skills.attack
        };
        foreach (var skill in new[] { skills.normal, skills.combo, skills.ultimate, skills.attack })
        {
            if (string.IsNullOrEmpty(skill)) continue;
            character.SkillInfo.LevelInfo.Add(new SKILL_LEVEL_INFO
            {
                SkillId = skill,
                SkillLevel = 1,
                SkillMaxLevel = 1,
                SkillEnhancedLevel = 1
            });
        }
    }


}
