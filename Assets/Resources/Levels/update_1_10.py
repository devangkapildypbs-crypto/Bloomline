import json
import os

dir_path = "c:/Users/devang.kapil/OneDrive - Markets and Markets/Documents/Bloomline - Puzzle Game/Assets/Resources/Levels"

def update_level(filename, text, fix_starts=True):
    path = os.path.join(dir_path, filename)
    if not os.path.exists(path):
        return
    with open(path, 'r') as f:
        data = json.load(f)
    
    data['tutorialText'] = text
    
    if fix_starts:
        solution_dict = {(s['x'], s['y']): s['correctRotation'] for s in data.get('solution', [])}
        
        for tile in data.get('tiles', []):
            pos = (tile['x'], tile['y'])
            if pos in solution_dict and not tile.get('locked', False):
                if tile['rotation'] == solution_dict[pos]:
                    tile['rotation'] = (tile['rotation'] + 90) % 360
                    
        # Update move targets
        moves_needed = 0
        for tile in data.get('tiles', []):
            pos = (tile['x'], tile['y'])
            if pos in solution_dict and not tile.get('locked', False):
                diff = abs(solution_dict[pos] - tile['rotation'])
                if diff == 270: diff = 90
                moves_needed += diff // 90
                
        data['moveTargetThreeStars'] = moves_needed
        data['moveTargetTwoStars'] = int(moves_needed * 1.5) + 1
        
        # fix level 10 duplicate tile bug
        if filename == 'level_010.json':
            seen = set()
            new_tiles = []
            for t in data['tiles']:
                pos = (t['x'], t['y'])
                if pos not in seen:
                    seen.add(pos)
                    new_tiles.append(t)
            data['tiles'] = new_tiles
            
    with open(path, 'w') as f:
        json.dump(data, f, indent=2)

update_level("level_001.json", "Welcome to the Garden! Tap the tile to rotate it and connect the source to the flower.")
update_level("level_002.json", "Great! Now try connecting multiple tiles.")
update_level("level_003.json", "Some tiles need multiple taps to find the right rotation.")
update_level("level_004.json", "Corner tiles connect two adjacent sides. Give them a spin!")
update_level("level_005.json", "Look at the shape of the path. Can you see where the light needs to go?")
update_level("level_006.json", "Think ahead! Imagine the path before you rotate the tiles.")
update_level("level_007.json", "Plan your moves carefully to earn 3 stars! Solve it with fewer taps.")
update_level("level_008.json", "The path is getting longer. Keep your moves efficient.")
update_level("level_009.json", "Long, winding paths ahead. Try to use as few taps as possible!")
update_level("level_010.json", "Dark tiles are blockers. Light cannot pass through them. Route around them!")

print("Updated 1-10")
