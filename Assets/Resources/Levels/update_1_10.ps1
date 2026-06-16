$levelsDir = "c:\Users\devang.kapil\OneDrive - Markets and Markets\Documents\Bloomline - Puzzle Game\Assets\Resources\Levels"
$texts = @{
    "level_001.json" = "Welcome to the Garden! Tap the tile to rotate it and connect the source to the flower."
    "level_002.json" = "Great! Now try connecting multiple tiles."
    "level_003.json" = "Some tiles need multiple taps to find the right rotation."
    "level_004.json" = "Corner tiles connect two adjacent sides. Give them a spin!"
    "level_005.json" = "Look at the shape of the path. Can you see where the light needs to go?"
    "level_006.json" = "Think ahead! Imagine the path before you rotate the tiles."
    "level_007.json" = "Plan your moves carefully to earn 3 stars! Solve it with fewer taps."
    "level_008.json" = "The path is getting longer. Keep your moves efficient."
    "level_009.json" = "Long, winding paths ahead. Try to use as few taps as possible!"
    "level_010.json" = "Dark tiles are blockers. Light cannot pass through them. Route around them!"
}

foreach ($key in $texts.Keys) {
    $path = Join-Path $levelsDir $key
    if (Test-Path $path) {
        $json = Get-Content -Raw $path | ConvertFrom-Json
        $json.tutorialText = $texts[$key]
        
        $solDict = @{}
        if ($json.solution) {
            foreach ($s in $json.solution) {
                $pos = "$($s.x),$($s.y)"
                $solDict[$pos] = $s.correctRotation
            }
        }
        
        $moves = 0
        if ($json.tiles) {
            $newTiles = @()
            $seen = @{}
            foreach ($t in $json.tiles) {
                $pos = "$($t.x),$($t.y)"
                if (-not $seen.ContainsKey($pos)) {
                    $seen[$pos] = $true
                    
                    if ($solDict.ContainsKey($pos) -and -not $t.locked) {
                        if ($t.rotation -eq $solDict[$pos]) {
                            $t.rotation = ($t.rotation + 90) % 360
                        }
                        $diff = [Math]::Abs($solDict[$pos] - $t.rotation)
                        if ($diff -eq 270) { $diff = 90 }
                        $moves += ($diff / 90)
                    }
                    $newTiles += $t
                }
            }
            $json.tiles = $newTiles
        }
        
        $json.moveTargetThreeStars = $moves
        $json.moveTargetTwoStars = [math]::Floor($moves * 1.5) + 1
        
        $json | ConvertTo-Json -Depth 10 | Set-Content $path
    }
}
Write-Output "Done"
