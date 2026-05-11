## Accomplished Goals

#### Codebase
- This project acts as an extension of the original project:
  - The new and modified code follows the existing coding style.
  - New functionality was added with minimal changes to the old systems.
  - Players can play both the original Match-3 mode and the newly added modes.
#### Re-skin
- By modifying the prefabs' textures
#### UI
- Added 1 extra screen for losing.
#### Changes/Improvements to Gameplay
- Added a new mode similar to Mahjong Match puzzle games, but instead of multiple layers, this game uses a single 6x4 board and a row of 5 slots used for holding selected tiles. Both have configurable sizes.
- The selected tiles placed in the holding row will be sorted automatically. Match 3 identical tiles at the same time to clear them (this number can be adjusted using configs).
- The player wins by clearing the entire board, and loses when every slot in the holding row is filled.
- The generation count of each tile type is a multiple of the match number, and the counts are kept as evenly distributed as possible.
- Aniamtions for valid moves and cleared tile matches.
- Timed Mode: The player must clear the board within 60 seconds. Filling the holding row does not result in a loss. Tiles in the holding row can be selected again to return them to their original positions.
- Extra features (made outside the 4-hour limit): 'AutoLose' and 'AutoWin' buttons in the pause menu. 'AutoLose' plays all tiles from bottom-left to top-right corner. 'AutoWin' only plays matching tile types, iterating through all types until the board is cleared.

---

## Game Previews

### Time Attack mode:

![timeAttack](https://github.com/taan41/Winter_Wolf_IEC_Intern_Test-Unity/blob/main/Assets/Readme/time_attack.gif)

### AutoWin:

![autoWin](https://github.com/taan41/Winter_Wolf_IEC_Intern_Test-Unity/blob/main/Assets/Readme/auto_win.gif)

---

## Code Changes Summary

### Gameplay related changes

#### Modified `GameSettings.cs`
- Added new fields for Mahjong settings

#### Modified `GameManager.cs`
- Modified enum `eLevelMode` to `{ MATCH_3_TIMER, MATCH_3_MOVES, MAHJONG_FULL_CLEAR, MAHJONG_TIME_ATTAKC }` to support new modes
- Added `GAME_WIN` to enum `eStateGame`
- Modifed method `LoadLevel(eLevelMode)` to load the newly added modes

#### Added `BoardMahjong.cs`
- Does not inherit from class `Board` due to sufficient differences in core structure/functionality
- Implements basic functions through methods `CreateBoard(bool)`, `Fill()`, `OnCellClicked(CellMahjong, Action)`, `FindMatchesAndShift(Action)`, `Clear()`
- Extra functions: `StartAutoWin()`, `StartAutoLose()`
- Contains event `OnAnimationFinished : Action` which is called after all animations are completed (moving item to bottom row, clearing and shifting items in bottom row)

#### Added `CellMahjong.cs`
- Inherits from class `Cell`
- Contains properties `IsBottom` and `AllowClicking` to support class `BoardMahjong` functionality

#### Modified 'Item.cs'
- Added property `OriginCellPos` to support class `BoardMahjong` functionality

#### Modified `LevelCondition.cs`
- Added event `ConditionFailedEven : Action` and method `OnConditionFailed()` to better differentiate winning & losing

#### Added `LevelMahjongFullClear.cs`
- Inherits from class `LevelCondition`
- Contains level condition checks in method `OnMoveFinished()`
  - Method `OnMoveFinished()` is subcribed to event `OnAnimationFinished` of class `BoardMahjong`
  - Calls base method `OnConditionCompleted()` when all items on the board and bottom row are cleared
  - Calls base method `OnConditionFailed()` when the bottom row is fully filled

#### Added `LevelMahjongTime.cs`
- Inherits from class `LevelCondition`
- Contains level condition checks in method `OnMoveFinished()` and `Update()`
  - Method `OnMoveFinished()` is subcribed to event `OnAnimationFinished` of class `BoardMahjong`
  - In `OnMoveFished()`, calls base method `OnConditionCompleted()` when all items on the board and bottom row are cleared
  - In `Update()`, calls base method `OnConditionFailed()` when timer runs out and total number of items on the board and bottom rows is bigger than 0

#### Modified `BoardController.cs`
- Renamed method `Update()` to `ManualUpdate()` since it was being called in method `Update()` in class `GameManager`
- Added field `m_mahjong : BoardMahjong`
- Added level mode checks: `IsMatch3()`, `IsMahjong()`
- Modified method `Update()` (now `ManualUpdate()`):
  - Showing hint and swapping items behaviors of Match 3 mode now goes through check `IsMatch3()`
  - Upon clicking on a cell, if check `IsMahjong()` returns true and cell object contains componet `CellMahjong`, method `OnCellClicked(CellMahjong, Action)` of field `m_mahjong` will be called
  - Also locks input until all animations are completed similarly to Match 3 mode 
 
### UI related changes

#### Modified `UIMainManager.cs`
- Centrailized level loading logic: use a single method `LoadLevel(GameManager.eLevelMode)` instead of different methods for each mode

#### Modified `UIPanelMain.cs`
- Added Button references `btnMahjongClear` and `btnMahjongTime`
- Added new methods similar to existing ones to support the new buttons

#### Added `UIPanelGameWin.cs`
- Behaves similarly to class `UIPanelGameOver.cs`, but has different main text: "Level Win!" instead of "Game Over!"

#### Modified `UIPanelPause.cs`
- Added Button references `btnAutoWin` and `btnAutoLose`

---

## Self Notes/Lessons

- Should've pushed the original project first to view changes more easily
- An interface/abstract class for board classes could've been made, but I didn't due to time constraint

---

## Screen Recordings
> Of the 4-hour process

[(1/2)](https://youtu.be/NLUOk9iKeuI)
[(2/2)](https://youtu.be/z2KzEdRKAZY)
