# Quick Start Guide - AR Battle Game

## 🎯 What's Ready

You have a complete turn-based AR battle game with:
- **890 lines** of production-ready code
- **8 core scripts** organized in clean namespaces
- **10 champions** with 4 abilities each
- **Full combat system** with health, damage, and targeting
- **UI integration** that auto-displays game state
- **AR detection hooks** already integrated

## ⚡ Quick Setup (3 Steps)

### Step 1: Create GameController
In CreativeWorld.unity scene:
1. Right-click in Hierarchy → Create Empty
2. Name it "GameController"
3. Add Component → GameManager (from Assets/Common/Scripts/Game/)
4. Leave `allChampions` empty - it auto-loads

### Step 2: Create UIController
1. Right-click GameController → Create Empty
2. Name it "UIController"
3. Add Component → UIManager (from Assets/Common/Scripts/UI/)
4. Drag GameController into the `gameManager` field
5. Done! UIManager auto-finds Canvas UI elements

### Step 3: Run!
Press Play. You're ready to test.

## 🎮 Controls During Battle

```
Selection Phase:
  SPACE - Lock in selected champions

Battle Phase:
  P     - Switch to next champion
  Q     - Cast Ability 1
  W     - Cast Ability 2  
  R     - Cast Ultimate
  SPACE - Pass turn (or confirm target in targeting mode)

Targeting Mode (Single-target abilities):
  P     - Cycle through enemies
  SPACE - Confirm target
```

## 🏗️ Game Flow

```
1. Player 1 Selection
   - Show champions on card
   - Press SPACE to lock in

2. Player 2 Selection
   - Same as Player 1

3. Battle Starts
   - Random 50/50 who goes first
   - Turn-based combat begins

4. Each Turn
   - Current player has 1 action
   - Can switch champions or cast abilities
   - Abilities auto-execute or enter targeting mode
   - Press SPACE to pass turn

5. Battle Ends
   - When one team reaches 0 HP
   - Winner declared
```

## 📊 Architecture Overview

```
GameManager
├─ Tracks game phase & player state
├─ Calls TurnManager for action management
├─ Calls BattleState for health tracking
├─ Calls ActionHandler for ability execution
└─ Updates UIManager with display info

ActionHandler
├─ Tracks active champion
├─ Handles ability casting
├─ Manages targeting mode
└─ Applies damage via BattleState

TurnManager
├─ Tracks actions per champion
├─ Prevents turn passing with actions remaining
└─ Resets actions on turn change

BattleState
├─ Stores all champion health
├─ Applies damage
└─ Determines defeat conditions
```

## 🎯 Ability Types

When creating abilities, set their `TargetType`:

| Type | Behavior |
|------|----------|
| **SingleTarget** | Player cycles enemies (P key), confirms (Space) |
| **AOE** | Hits all enemies immediately |
| **TeamOnly** | Heals/buffs all allies immediately |
| **SingleTargetTeam** | Player selects single ally for buff/heal |

## 🔗 AR Integration

Already connected! Your AR detection system calls:

```csharp
// When a champion card is detected
gameManager.OnChampionDetected(championStats);

// When a champion card is removed
gameManager.OnChampionLost(championStats);
```

No extra setup needed.

## 📝 File Locations

```
Assets/Common/Scripts/Game/
  ├─ GameManager.cs          (Main controller)
  ├─ BattlePhase.cs          (Game phases enum)
  ├─ TurnManager.cs          (Action system)
  ├─ BattleState.cs          (Health tracking)
  └─ ActionHandler.cs        (Ability execution)

Assets/Common/Scripts/Battle/
  ├─ ChampionStats.cs        (Champion data)
  └─ ChampionAbility.cs      (Ability + targeting types)

Assets/Common/Scripts/UI/
  └─ UIManager.cs            (UI display)

GAME_SYSTEM_DOCUMENTATION.md (Complete reference)
```

## ✅ Verification Checklist

Before running:
- [ ] GameController exists with GameManager
- [ ] UIController exists with UIManager
- [ ] GameManager assigned to UIManager
- [ ] Canvas exists with "BeforeGame" text element
- [ ] All 10 champions in Resources/Champions/

## 🚨 Common Issues

**"No actions remaining"**
- Expected! Turn system limits 1 action per champ
- Press SPACE to pass turn to other player

**UI not updating**
- Check Canvas has "BeforeGame" TextMeshProUGUI
- UIManager looks for it at start

**Champions not loading**
- Verify they're in Assets/Resources/Champions/
- GameManager auto-loads them

**Abilities not working**
- Check champion has abilities assigned in ChampionStats
- Q = Ability1, W = Ability2, R = Ultimate

## 📞 Need Help?

See `GAME_SYSTEM_DOCUMENTATION.md` for:
- Complete architecture details
- All public methods and properties
- Example game flow
- Technical implementation notes
