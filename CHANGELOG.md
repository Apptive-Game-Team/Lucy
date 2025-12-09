# Changelog

## [Debug & Refactor Update] - 2024-12-09

### 🐛 Bug Fixes
- Fixed null reference exception in `SingletonObject.Instance` when FindObjectOfType returns null
- Fixed null reference exception in `ReferenceManager.FindGameObjectByName` with proper validation
- Fixed null reference exception in `Inventory.IsItemEquipped` when checking equipped items
- Fixed incorrect yMax calculation in `CreatureManager.PointsInCircle` (was using cx instead of cy)
- Fixed typo in GitHub workflow file (devalop -> develop)
- Removed unused using statements in multiple files:
  - `SingletonObject.cs` (removed Unity.IO.LowLevel.Unsafe)
  - `EventObject.cs` (removed System.Collections and System.Collections.Generic)
  - `PuzzleManager2F.cs` (removed UnityEditor.SearchService)

### ♻️ Refactoring
- Improved bounds checking in `CreatureManager.ApplyDoorOnMap` and `ApplyLightOnMap`
  - Replaced try-catch with explicit bounds validation
  - Added warning logs for out-of-bounds positions
- Consolidated duplicate code in `Inventory` equip/unequip logic
  - Extracted `HandleItemSpecificEquipLogic` method for flashlight logic
  - Reduced code duplication between Equip and UnEquip methods
- Extracted magic numbers to named constants:
  - `CharacterStat.cs`: MENTAL_WARNING_RATE, STAMINA_RECOVERY_THRESHOLD, SPOTLIGHT_EFFECT_DURATION
  - `CharacterMove.cs`: RUN_SPEED_MULTIPLIER, RUN_ANIMATION_SPEED, STAMINA_DRAIN_RATE, STAMINA_RECOVERY_RATE
- Removed commented-out dead code in `CharacterStat.UpdateStats`
- Added missing using statement in `PortalManager` (System.Collections.Generic)

### 📚 Documentation
- Added comprehensive XML documentation to core systems:
  - `SingletonObject<T>`: Singleton pattern lifecycle and usage
  - `PortalManager`: Scene transition and listener management
  - `CharacterStat`: Mental and stamina system mechanics
  - `InputManager`: Input management and key binding system
  - `ReferenceManager`: Global object reference system
  - `Inventory`: Item management and equipment system
  - `Creature`: AI behavior states and pathfinding
  - `CreatureManager`: Map management and pathfinding services

- Enhanced inline documentation:
  - Added comments to mental reduction algorithm in `CharacterStat`
  - Documented creature detection logic and state transitions
  - Documented pathfinding methods (SetPathToPosition, SetDirectionPath, SetRandomPath)
  - Documented map generation and obstacle application logic

- Updated README.md:
  - Added game features overview
  - Added architecture overview with system descriptions
  - Added directory structure documentation
  - Added code examples for common patterns:
    - Singleton pattern usage
    - Scene change listener implementation
    - Character stat management
    - Inventory operations
    - Reference manager usage

- Created CHANGELOG.md to track project changes

### 🏗️ Architecture Improvements
- Standardized error handling with explicit null checks and warning logs
- Improved code organization with helper methods
- Enhanced maintainability with better variable names and constants
- Improved code readability with comprehensive documentation

### 🔒 Security
- Ran CodeQL security analysis - **0 vulnerabilities found**
- All code changes reviewed for security implications
- Proper null safety checks added throughout

### 📝 Notes
- All changes maintain backward compatibility
- No breaking changes to existing APIs
- All existing functionality preserved
- TypeScript code quality improvements throughout
