$levelsDir = "c:\Users\devang.kapil\OneDrive - Markets and Markets\Documents\Bloomline - Puzzle Game\Assets\Resources\Levels"

function Export-Level {
    param($levelNum, $chapter, $tut, $w, $h, $paths, $blockers, $locked)
    
    $tiles = @()
    $solution = @()
    $moves = 0
    
    # Process paths
    foreach ($path in $paths) {
        $pts = $path.pts
        $col = $path.col
        
        for ($i=0; $i -lt $pts.Count; $i++) {
            $x = $pts[$i][0]
            $y = $pts[$i][1]
            $pos = "$x,$y"
            
            if ($i -eq 0) {
                # Source
                $nx = $pts[1][0]; $ny = $pts[1][1]
                if ($nx -gt $x) { $rot = 0 }
                elseif ($ny -lt $y) { $rot = 90 }
                elseif ($nx -lt $x) { $rot = 180 }
                elseif ($ny -gt $y) { $rot = 270 }
                
                $tiles += @{ x=$x; y=$y; type="Source"; rotation=$rot; color=$col; locked=$true }
            }
            elseif ($i -eq $pts.Count - 1) {
                # Flower
                $px = $pts[$i-1][0]; $py = $pts[$i-1][1]
                if ($px -lt $x) { $rot = 0 }
                elseif ($py -lt $y) { $rot = 90 }
                elseif ($px -gt $x) { $rot = 180 }
                elseif ($py -gt $y) { $rot = 270 }
                
                $tiles += @{ x=$x; y=$y; type="Flower"; rotation=$rot; color=$col; locked=$true }
            }
            else {
                # Path tile
                $px = $pts[$i-1][0]; $py = $pts[$i-1][1]
                $nx = $pts[$i+1][0]; $ny = $pts[$i+1][1]
                
                $sides = @()
                if ($px -lt $x -or $nx -lt $x) { $sides += "W" }
                if ($px -gt $x -or $nx -gt $x) { $sides += "E" }
                if ($py -lt $y -or $ny -lt $y) { $sides += "S" }
                if ($py -gt $y -or $ny -gt $y) { $sides += "N" }
                
                $type = "Corner"
                $correctRot = 0
                
                if ("W" -in $sides -and "E" -in $sides) { $type = "Straight"; $correctRot = 90 }
                elseif ("N" -in $sides -and "S" -in $sides) { $type = "Straight"; $correctRot = 0 }
                elseif ("N" -in $sides -and "E" -in $sides) { $type = "Corner"; $correctRot = 0 }
                elseif ("E" -in $sides -and "S" -in $sides) { $type = "Corner"; $correctRot = 90 }
                elseif ("S" -in $sides -and "W" -in $sides) { $type = "Corner"; $correctRot = 180 }
                elseif ("W" -in $sides -and "N" -in $sides) { $type = "Corner"; $correctRot = 270 }
                
                $isLocked = $false
                if ($locked -contains $pos) {
                    $isLocked = $true
                    $type = "Locked" + $type
                }
                
                $startRot = $correctRot
                if (-not $isLocked) {
                    $startRot = ($correctRot + 90) % 360
                    $diff = [Math]::Abs($correctRot - $startRot)
                    if ($diff -eq 270) { $diff = 90 }
                    $moves += ($diff / 90)
                    
                    $solution += @{ x=$x; y=$y; correctRotation=$correctRot }
                }
                
                $tiles += @{ x=$x; y=$y; type=$type; rotation=$startRot; color=$col; locked=$isLocked }
            }
        }
    }
    
    if ($blockers) {
        foreach ($b in $blockers) {
            $tiles += @{ x=$b[0]; y=$b[1]; type="Blocker"; rotation=0; color="White"; locked=$true }
        }
    }
    
    $json = @{
        levelId = "level_$("{0:d3}" -f $levelNum)"
        chapter = $chapter
        levelNumber = $levelNum
        gridWidth = $w
        gridHeight = $h
        moveTargetThreeStars = $moves
        moveTargetTwoStars = [math]::Floor($moves * 1.5) + 1
        tutorialText = $tut
        tiles = $tiles
        solution = $solution
    }
    
    $path = Join-Path $levelsDir "level_$("{0:d3}" -f $levelNum).json"
    $json | ConvertTo-Json -Depth 10 | Set-Content $path
}

# --- DEFINE LEVELS ---

$configs = @()

# Chapter 2: The Grove (Levels 21-35)
$configs += @{ n=21; c=2; w=5; h=5; t="Welcome to the Grove! Red light only activates Red flowers."; paths=@(
    @{ col="Red"; pts=@((0,2), (1,2), (2,2), (3,2), (4,2)) }
)}
$configs += @{ n=22; c=2; w=5; h=5; t="Blue light connects to Blue flowers."; paths=@(
    @{ col="Blue"; pts=@((1,4), (1,3), (1,2), (2,2), (3,2), (3,1)) }
)}
$configs += @{ n=23; c=2; w=5; h=5; t="Manage both colors simultaneously."; paths=@(
    @{ col="Red"; pts=@((0,4), (1,4), (2,4), (3,4), (4,4)) },
    @{ col="Blue"; pts=@((0,1), (1,1), (2,1), (3,1), (4,1)) }
)}
$configs += @{ n=24; c=2; w=5; h=5; t="Sometimes paths need to weave around each other."; paths=@(
    @{ col="Red"; pts=@((0,3), (1,3), (1,4), (2,4), (3,4), (4,4)) },
    @{ col="Blue"; pts=@((0,1), (1,1), (2,1), (2,2), (2,3), (3,3), (4,3)) }
)}
$configs += @{ n=25; c=2; w=5; h=5; t="Blockers force you into tighter spaces."; paths=@(
    @{ col="White"; pts=@((0,2), (1,2), (1,1), (2,1), (3,1), (3,2), (4,2)) }
); blk=@((2,2)) }
$configs += @{ n=26; c=2; w=5; h=5; t="Red and Blue paths sharing a grid."; paths=@(
    @{ col="Red"; pts=@((0,4), (0,3), (1,3), (2,3), (2,4), (3,4), (4,4)) },
    @{ col="Blue"; pts=@((0,0), (1,0), (2,0), (3,0), (4,0)) }
)}
$configs += @{ n=27; c=2; w=6; h=5; t="A larger area means longer paths."; paths=@(
    @{ col="Red"; pts=@((0,2), (1,2), (1,3), (2,3), (3,3), (4,3), (5,3)) },
    @{ col="Blue"; pts=@((1,0), (1,1), (2,1), (3,1), (4,1), (4,2), (5,2)) }
)}
$configs += @{ n=28; c=2; w=6; h=5; t="Cross them carefully!"; paths=@(
    @{ col="Red"; pts=@((0,4), (1,4), (1,3), (1,2), (2,2), (3,2), (4,2)) },
    @{ col="Blue"; pts=@((0,0), (1,0), (2,0), (2,1), (3,1), (3,0), (4,0), (5,0)) }
)}
$configs += @{ n=29; c=2; w=6; h=6; t="Multiple flowers require multiple sources."; paths=@(
    @{ col="Red"; pts=@((0,5), (1,5), (2,5), (3,5)) },
    @{ col="Blue"; pts=@((0,3), (1,3), (2,3), (3,3), (4,3)) },
    @{ col="White"; pts=@((0,1), (1,1), (2,1), (3,1), (4,1), (5,1)) }
)}
$configs += @{ n=30; c=2; w=6; h=6; t="Wind your way through the blockers."; paths=@(
    @{ col="Blue"; pts=@((1,5), (1,4), (2,4), (3,4), (3,5), (4,5)) }
); blk=@((2,5), (1,3), (3,3), (4,4)) }
$configs += @{ n=31; c=2; w=6; h=6; t="Keep the colors separated."; paths=@(
    @{ col="Red"; pts=@((0,2), (0,3), (1,3), (2,3), (3,3), (3,2), (4,2)) },
    @{ col="Blue"; pts=@((0,0), (1,0), (2,0), (3,0), (4,0), (5,0)) }
)}
$configs += @{ n=32; c=2; w=6; h=6; t="Tight corners."; paths=@(
    @{ col="Red"; pts=@((0,5), (0,4), (1,4), (1,5), (2,5), (3,5), (3,4), (4,4)) },
    @{ col="Red"; pts=@((0,1), (1,1), (2,1), (2,2), (3,2), (4,2)) }
)}
$configs += @{ n=33; c=2; w=6; h=6; t="The Grove grows dense."; paths=@(
    @{ col="Blue"; pts=@((1,1), (1,2), (1,3), (2,3), (3,3), (4,3), (4,2), (4,1)) }
); blk=@((2,2), (3,2)) }
$configs += @{ n=34; c=2; w=6; h=6; t="Double U-turns."; paths=@(
    @{ col="Red"; pts=@((0,2), (1,2), (1,3), (0,3), (0,4), (1,4), (2,4), (3,4)) },
    @{ col="Blue"; pts=@((5,3), (4,3), (4,2), (5,2), (5,1), (4,1), (3,1), (2,1)) }
)}
$configs += @{ n=35; c=2; w=6; h=6; t="The final challenge of The Grove!"; paths=@(
    @{ col="Red"; pts=@((0,5), (1,5), (2,5), (2,4), (3,4), (4,4), (4,5)) },
    @{ col="Blue"; pts=@((0,0), (1,0), (2,0), (2,1), (3,1), (4,1), (4,0)) },
    @{ col="White"; pts=@((0,3), (1,3), (1,2), (2,2), (3,2), (4,2), (5,2)) }
); blk=@((3,5), (3,0)) }

# Chapter 3: The Sanctuary (Levels 36-50)
$configs += @{ n=36; c=3; w=5; h=5; t="Welcome to the Sanctuary! Some tiles are locked in place."; paths=@(
    @{ col="White"; pts=@((0,2), (1,2), (2,2), (3,2), (4,2)) }
); lck=@("2,2") }
$configs += @{ n=37; c=3; w=5; h=5; t="Work around the locked constraints."; paths=@(
    @{ col="Red"; pts=@((0,1), (1,1), (1,2), (2,2), (3,2), (3,3), (4,3)) }
); lck=@("2,2") }
$configs += @{ n=38; c=3; w=6; h=5; t="Locked corners can be tricky."; paths=@(
    @{ col="Blue"; pts=@((0,4), (1,4), (1,3), (2,3), (3,3), (3,4), (4,4)) }
); lck=@("1,4", "3,4") }
$configs += @{ n=39; c=3; w=6; h=5; t="More paths, more locks."; paths=@(
    @{ col="Red"; pts=@((0,3), (1,3), (2,3), (3,3), (4,3)) },
    @{ col="Blue"; pts=@((0,1), (1,1), (2,1), (3,1), (4,1)) }
); lck=@("2,3", "2,1") }
$configs += @{ n=40; c=3; w=6; h=5; t="Blockers and locks together."; paths=@(
    @{ col="White"; pts=@((0,2), (1,2), (1,1), (2,1), (3,1), (4,1), (4,2), (5,2)) }
); blk=@((2,2), (3,2)); lck=@("1,1", "4,1") }
$configs += @{ n=41; c=3; w=6; h=6; t="A true Sanctuary puzzle."; paths=@(
    @{ col="Red"; pts=@((0,5), (0,4), (1,4), (2,4), (2,5), (3,5), (4,5)) },
    @{ col="Blue"; pts=@((0,1), (0,2), (1,2), (2,2), (2,1), (3,1), (4,1)) }
); lck=@("1,4", "1,2") }
$configs += @{ n=42; c=3; w=6; h=6; t="Zig-zag through the locks."; paths=@(
    @{ col="White"; pts=@((0,0), (1,0), (1,1), (2,1), (2,2), (3,2), (3,3), (4,3), (4,4), (5,4)) }
); lck=@("1,1", "3,3") }
$configs += @{ n=43; c=3; w=6; h=6; t="Multiple sources with locked centers."; paths=@(
    @{ col="Red"; pts=@((0,4), (1,4), (2,4), (3,4), (4,4), (5,4)) },
    @{ col="Blue"; pts=@((0,2), (1,2), (2,2), (3,2), (4,2), (5,2)) }
); lck=@("2,4", "3,2") }
$configs += @{ n=44; c=3; w=6; h=6; t="Don't get blocked in."; paths=@(
    @{ col="White"; pts=@((0,3), (1,3), (1,4), (2,4), (3,4), (4,4), (4,3), (5,3)) }
); blk=@((2,3), (3,3)); lck=@("1,4", "4,4") }
$configs += @{ n=45; c=3; w=6; h=6; t="Three colors, three locks."; paths=@(
    @{ col="Red"; pts=@((0,5), (1,5), (2,5), (3,5)) },
    @{ col="Blue"; pts=@((0,3), (1,3), (2,3), (3,3)) },
    @{ col="White"; pts=@((0,1), (1,1), (2,1), (3,1)) }
); lck=@("1,5", "2,3", "1,1") }
$configs += @{ n=46; c=3; w=7; h=6; t="Larger grid, tighter paths."; paths=@(
    @{ col="Red"; pts=@((0,4), (1,4), (1,3), (2,3), (3,3), (4,3), (4,4), (5,4)) },
    @{ col="Blue"; pts=@((0,2), (1,2), (1,1), (2,1), (3,1), (4,1), (4,2), (5,2)) }
); lck=@("1,3", "4,3", "1,1", "4,1") }
$configs += @{ n=47; c=3; w=7; h=6; t="Snaking paths everywhere."; paths=@(
    @{ col="White"; pts=@((0,5), (0,4), (1,4), (1,3), (2,3), (2,2), (3,2), (3,1), (4,1), (4,0), (5,0)) }
); lck=@("1,4", "2,3", "3,2", "4,1") }
$configs += @{ n=48; c=3; w=7; h=6; t="Almost there! Master the locks."; paths=@(
    @{ col="Red"; pts=@((1,5), (1,4), (2,4), (3,4), (4,4), (4,5), (5,5)) },
    @{ col="Blue"; pts=@((1,1), (1,2), (2,2), (3,2), (4,2), (4,1), (5,1)) }
); blk=@((2,5), (3,5), (2,1), (3,1)); lck=@("1,4", "4,4", "1,2", "4,2") }
$configs += @{ n=49; c=3; w=7; h=7; t="The Penultimate Challenge."; paths=@(
    @{ col="Red"; pts=@((0,6), (1,6), (2,6), (3,6), (4,6), (5,6), (6,6)) },
    @{ col="Blue"; pts=@((0,4), (1,4), (2,4), (3,4), (4,4), (5,4), (6,4)) },
    @{ col="White"; pts=@((0,2), (1,2), (2,2), (3,2), (4,2), (5,2), (6,2)) }
); lck=@("3,6", "3,4", "3,2"); blk=@((3,5), (3,3)) }
$configs += @{ n=50; c=3; w=7; h=7; t="The Sanctuary's Core! Combine everything."; paths=@(
    @{ col="Red"; pts=@((0,6), (1,6), (1,5), (2,5), (3,5), (4,5), (4,6), (5,6), (6,6)) },
    @{ col="Blue"; pts=@((0,0), (1,0), (1,1), (2,1), (3,1), (4,1), (4,0), (5,0), (6,0)) },
    @{ col="White"; pts=@((0,3), (1,3), (2,3), (3,3), (4,3), (5,3), (6,3)) }
); lck=@("1,5", "4,5", "1,1", "4,1", "3,3"); blk=@((3,6), (3,0)) }

foreach ($cfg in $configs) {
    Export-Level -levelNum $cfg.n -chapter $cfg.c -tut $cfg.t -w $cfg.w -h $cfg.h -paths $cfg.paths -blockers $cfg.blk -locked $cfg.lck
}

Write-Output "Generated 21-50"
