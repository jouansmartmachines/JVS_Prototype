Theme System Guide :
- Etape 1 :
	Créer un ThemeManager (Create/Game/Theme/ThemeManager) nomenclature : [NOM_DU_JEU]_ThemeManager (La nomenclature est très importante pour que tout s'auto assigne correctement, aller voir CovidKiller/Theme pour référence) 
- Etape 2 :
	Créer le GameTheme (Create/Game/Theme/GameTheme) par default (ex: pour CovidKiller, le Thème par default est Covid) nomenclature : [NOM_DU_JEU]_Theme_[NOM_DU_THEME] (La nomenclature est très importante pour que tout s'auto assigne correctement, aller voir CovidKiller/Theme pour référence)
- Etape 3 :
	Pour chaque object pour vous voulez changer quand on switch de Thème, créer un SwapObject (Create/Game/Theme/SwapObject) nomenclature : [NOM_DU_JEU]_[NOM_DE_L'ELEMENT] (La nomenclature est très importante pour que tout s'auto assigne correctement, aller voir CovidKiller/Theme pour référence)
- Etape 4 :
	Maintenant il faut créer tout les éléments pour le Thème par default (ex: Si vous voulez changer un sprite, vous avez donc créer un SwapObject pour ce sprite et maintenant pour chaque Thème il vous faudra créer un SwapSprite (Create/Game/Theme/Entity/SwapSprite) qui doit être renseigner dans le GameTheme et SwapObject correspondent, si vous avez bien nommé vos éléments, l'assignation se fera automatiquement, nomenclature : [NOM_DU_JEU]_Theme_[NOM_DU_THEME]_[NOM_DE_L'ELEMENT])
- Etape 5 :
	Il vous faut maintenant modifier le jeu pour qu'il utilise les SwapObject que vous avez créer au lieu des objects (Prefabs, Sprites, Animations …). 
Pour cela vous avez plusieurs scripts monobehaviour (Assets/Universal/Theme/Scripts/Behaviour) qui vous permet de changé les éléments qui sont juste dans la scène.
Pour les éléments utilisé en scripts il vous faudra modifier directement les scripts (ex: Si vous voulez utiliser une prefab différentes à chaque Thème, assigner le SwapObject de votre préfab puis au moment d'utiliser la prefab, appel SwapObject.GetSwapEntity<SwapPrefab>().Prefab
- Etape 6 :
	Pour pouvoir changé entre les Thèmes, il vous suffit de mettre un ThemeSelector (Assets/Universal/Theme/Prefabs/Theme Selector.prefab) dans le menu du jeu et y renseigner le ThemeManager de votre jeu
- Etape 7 :
	Maintenant plus qu'a refaire l'étape 2 et 4 en boucle pour chaque Thème que vous voulez créer