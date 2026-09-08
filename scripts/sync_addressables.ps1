param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

function Get-Guid([string]$assetPath) {
    $metaPath = "$assetPath.meta"
    if (-not (Test-Path -LiteralPath $metaPath)) {
        throw "Falta el archivo meta de $assetPath"
    }

    $match = [regex]::Match([IO.File]::ReadAllText($metaPath), '(?m)^guid:\s*([a-f0-9]{32})\s*$')
    if (-not $match.Success) {
        throw "No se encontró un GUID válido en $metaPath"
    }
    return $match.Groups[1].Value
}

function Normalize-Name([string]$value) {
    return $value.Trim().Replace(' ', '_').Replace('-', '_').ToLowerInvariant()
}

function New-Entry([string]$assetPath, [string]$address, [string[]]$labels) {
    return [pscustomobject]@{
        Guid = Get-Guid $assetPath
        Address = $address
        Labels = $labels
    }
}

function Get-Assets([string]$relativeFolder, [string[]]$extensions) {
    $folder = Join-Path $ProjectRoot $relativeFolder
    if (-not (Test-Path -LiteralPath $folder)) { return @() }
    return @(Get-ChildItem -LiteralPath $folder -Recurse -File | Where-Object {
        $extensions -contains $_.Extension.ToLowerInvariant()
    } | Sort-Object FullName)
}

function Set-SpriteImporterMeta([IO.FileInfo]$file, [int]$maxTextureSize, [hashtable]$borders) {
    $metaPath = "$($file.FullName).meta"
    if (-not (Test-Path -LiteralPath $metaPath)) { return }

    $content = [IO.File]::ReadAllText($metaPath)
    if ($content -notmatch '(?m)^TextureImporter:$') { return }

    $border = if ($borders.ContainsKey($file.BaseName)) { $borders[$file.BaseName] } else { '0, 0, 0, 0' }
    $content = [regex]::Replace($content, '(?m)^    enableMipMap: \d+$', '    enableMipMap: 0')
    $content = [regex]::Replace($content, '(?m)^    wrapU: \d+$', '    wrapU: 1')
    $content = [regex]::Replace($content, '(?m)^    wrapV: \d+$', '    wrapV: 1')
    $content = [regex]::Replace($content, '(?m)^  nPOTScale: \d+$', '  nPOTScale: 0')
    if ($content -notmatch '(?m)^  spriteMode: 2$') {
        $content = [regex]::Replace($content, '(?m)^  spriteMode: \d+$', '  spriteMode: 1')
    }
    $content = [regex]::Replace($content, '(?m)^  spriteMeshType: \d+$', '  spriteMeshType: 1')
    $content = [regex]::Replace($content, '(?m)^  spriteBorder: .+$', "  spriteBorder: {x: $($border.Split(',')[0].Trim()), y: $($border.Split(',')[1].Trim()), z: $($border.Split(',')[2].Trim()), w: $($border.Split(',')[3].Trim())}")
    $content = [regex]::Replace($content, '(?m)^  spriteGenerateFallbackPhysicsShape: \d+$', '  spriteGenerateFallbackPhysicsShape: 0')
    $content = [regex]::Replace($content, '(?m)^  alphaIsTransparency: \d+$', '  alphaIsTransparency: 1')
    $content = [regex]::Replace($content, '(?m)^  textureType: \d+$', '  textureType: 8')
    $content = [regex]::Replace($content, '(?m)^  maxTextureSize: \d+$', "  maxTextureSize: $maxTextureSize", 1)
    if ($content -match '(?m)^  spriteMode: 1$') {
        $content = [regex]::Replace($content, '(?m)^    spriteID:\s*$', '    spriteID: 5e97eb03825dee720800000000000000')
    }
    [IO.File]::WriteAllText($metaPath, $content, [Text.UTF8Encoding]::new($false))
}

function Set-ContentSpriteImporters {
    $borders = @{
        'ui_panel_rounded_16' = '24, 24, 24, 24'
        'ui_button_rounded_14' = '20, 20, 20, 20'
        'ui_input_rounded_12' = '18, 18, 18, 18'
        'ui_pill' = '32, 32, 32, 32'
        'ui_progress_rounded' = '15, 15, 15, 15'
        'ui_card_shadow' = '40, 44, 40, 40'
        'ui_modal_shadow' = '48, 52, 48, 44'
    }

    foreach ($file in Get-Assets 'Assets/Content/UI' @('.png')) {
        Set-SpriteImporterMeta $file 512 $borders
    }
    foreach ($file in Get-Assets 'Assets/Content/Imagenes' @('.png')) {
        Set-SpriteImporterMeta $file 2048 $borders
    }
}

function Set-GroupEntries([string]$groupFile, [object[]]$entries, [string]$groupName = '') {
    $path = Join-Path $ProjectRoot "Assets/AddressableAssetsData/AssetGroups/$groupFile"
    $content = [IO.File]::ReadAllText($path)

    if ($groupName) {
        $content = [regex]::Replace($content, '(?m)^  m_Name: .+$', "  m_Name: $groupName", 1)
        $content = [regex]::Replace($content, '(?m)^  m_GroupName: .+$', "  m_GroupName: $groupName", 1)
    }

    $builder = [Text.StringBuilder]::new()
    [void]$builder.AppendLine('  m_SerializeEntries:')
    foreach ($entry in ($entries | Sort-Object Address)) {
        [void]$builder.AppendLine("  - m_GUID: $($entry.Guid)")
        [void]$builder.AppendLine("    m_Address: $($entry.Address)")
        [void]$builder.AppendLine('    m_ReadOnly: 0')
        if ($entry.Labels.Count -eq 0) {
            [void]$builder.AppendLine('    m_SerializedLabels: []')
        } else {
            [void]$builder.AppendLine('    m_SerializedLabels:')
            foreach ($label in $entry.Labels) {
                [void]$builder.AppendLine("    - $label")
            }
        }
        [void]$builder.AppendLine('    FlaggedDuringContentUpdateRestriction: 0')
    }

    $replacement = $builder.ToString()
    $entryPattern = '(?ms)^  m_SerializeEntries:.*?(?=^  m_ReadOnly:)'
    if (-not [regex]::IsMatch($content, $entryPattern)) {
        throw "No se pudo actualizar la lista de entradas de $groupFile"
    }
    $updated = [regex]::Replace($content, $entryPattern, $replacement)
    [IO.File]::WriteAllText($path, $updated, [Text.UTF8Encoding]::new($false))
}

Set-ContentSpriteImporters

$dataEntries = foreach ($file in Get-Assets 'Assets/Content/Data' @('.json')) {
    $name = Normalize-Name $file.BaseName
    $address = if ($name -eq 'lecciones_compiladas') { 'json_lecciones' } else { "json_$name" }
    New-Entry $file.FullName $address @('global_data')
}

$audioEntries = @()
foreach ($file in Get-Assets 'Assets/Content/Audio/Palabras' @('.wav')) {
    $audioEntries += New-Entry $file.FullName "audio_palabra/$(Normalize-Name $file.BaseName)" @('audio_palabras')
}
foreach ($file in Get-Assets 'Assets/Content/Audio/Oraciones' @('.wav')) {
    $audioEntries += New-Entry $file.FullName "audio_oracion/$($file.BaseName)" @('audio_oraciones')
}

$uiEntries = @()
$themePath = Join-Path $ProjectRoot 'Assets/Content/UI/UITheme.asset'
if (Test-Path -LiteralPath $themePath) {
    $uiEntries += New-Entry $themePath 'UITheme' @('ui_theme')
}
foreach ($file in Get-Assets 'Assets/Content/UI/Base' @('.png')) {
    $uiEntries += New-Entry $file.FullName "ui/base/$(Normalize-Name $file.BaseName)" @('ui', 'ui_base')
}
foreach ($file in Get-Assets 'Assets/Content/UI/Effects' @('.png')) {
    $uiEntries += New-Entry $file.FullName "ui/effect/$(Normalize-Name $file.BaseName)" @('ui', 'ui_effects')
}
foreach ($file in Get-Assets 'Assets/Content/UI/Fonts' @('.asset', '.ttf', '.otf')) {
    $uiEntries += New-Entry $file.FullName "ui/font/$(Normalize-Name $file.BaseName)" @('ui', 'ui_fonts')
}
foreach ($file in Get-Assets 'Assets/Content/UI/Icons' @('.png')) {
    $category = Normalize-Name $file.Directory.Name
    $uiEntries += New-Entry $file.FullName "ui/icon/$category/$(Normalize-Name $file.BaseName)" @('ui', 'ui_icons')
}

$imageEntries = @()
foreach ($file in Get-Assets 'Assets/Content/Imagenes/Avatares' @('.png')) {
    $imageEntries += New-Entry $file.FullName "avatar/$(Normalize-Name $file.BaseName)" @('avatars')
}
foreach ($mapping in @(
    @{ Folder = 'Fondos'; Prefix = 'imagen/fondo'; Label = 'backgrounds' },
    @{ Folder = 'Marca'; Prefix = 'imagen/marca'; Label = 'branding' },
    @{ Folder = 'Islas'; Prefix = 'imagen/isla'; Label = 'islands' }
)) {
    foreach ($file in Get-Assets "Assets/Content/Imagenes/$($mapping.Folder)" @('.png', '.jpg', '.jpeg')) {
        $imageEntries += New-Entry $file.FullName "$($mapping.Prefix)/$(Normalize-Name $file.BaseName)" @($mapping.Label)
    }
}

$prefabEntries = foreach ($file in Get-Assets 'Assets/Content/Prefabs' @('.prefab')) {
    $name = Normalize-Name $file.BaseName
    $address = switch ($name) {
        'boton' { 'prefab_boton' }
        'islaleccion' { 'prefab_isla_leccion' }
        'wordcard' { 'prefab_word_card' }
        'actividademparejamiento' { 'actividad_emparejamiento' }
        'actividadseleccion' { 'actividad_seleccion' }
        'pln_palabra' { 'pln_palabra' }
        'nubedialogo' { 'prefab_nube_dialogo' }
        default { "prefab/$name" }
    }
    New-Entry $file.FullName $address @('prefab')
}

$raEntries = foreach ($file in Get-Assets 'Assets/Content/Modelos3D' @('.fbx')) {
    $name = Normalize-Name $file.BaseName
    $address = if ($name -eq 'ra_saludos_personaje_base') { 'personaje_base' } else { "ra/modelo/$name" }
    New-Entry $file.FullName $address @('ra', '3D')
}

$allEntries = @($dataEntries) + @($audioEntries) + @($uiEntries) + @($imageEntries) + @($prefabEntries) + @($raEntries)
$duplicateAddresses = $allEntries | Group-Object Address | Where-Object Count -gt 1
$duplicateGuids = $allEntries | Group-Object Guid | Where-Object Count -gt 1
if ($duplicateAddresses -or $duplicateGuids) {
    throw 'Se detectaron direcciones o GUID duplicados al generar el catálogo.'
}

Set-GroupEntries 'Global_Data.asset' @($dataEntries)
Set-GroupEntries 'Audios.asset' @($audioEntries)
Set-GroupEntries 'Iconos.asset' @($uiEntries) 'UI'
Set-GroupEntries 'Imagenes.asset' @($imageEntries)
Set-GroupEntries 'Prefabs.asset' @($prefabEntries)
Set-GroupEntries 'RA.asset' @($raEntries)

$settingsPath = Join-Path $ProjectRoot 'Assets/AddressableAssetsData/AddressableAssetSettings.asset'
$settings = [IO.File]::ReadAllText($settingsPath)
$labels = $allEntries.Labels | ForEach-Object { $_ } | Sort-Object -Unique
$labelBlock = [Text.StringBuilder]::new()
[void]$labelBlock.AppendLine('  m_LabelTable:')
[void]$labelBlock.AppendLine('    m_LabelNames:')
foreach ($label in $labels) {
    [void]$labelBlock.AppendLine("    - $label")
}
$settings = [regex]::Replace(
    $settings,
    '(?ms)^  m_LabelTable:.*?(?=^  m_SchemaTemplates:)',
    $labelBlock.ToString())
[IO.File]::WriteAllText($settingsPath, $settings, [Text.UTF8Encoding]::new($false))

Write-Output "Addressables sincronizados: $($allEntries.Count) recursos."
Write-Output "Datos=$(@($dataEntries).Count), Audios=$(@($audioEntries).Count), UI=$(@($uiEntries).Count), Imagenes=$(@($imageEntries).Count), Prefabs=$(@($prefabEntries).Count), RA=$(@($raEntries).Count)"
