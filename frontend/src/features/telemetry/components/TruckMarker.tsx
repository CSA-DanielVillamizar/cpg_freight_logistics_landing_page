import L from 'leaflet';

/**
 * A rotated truck glyph rendered as a Leaflet DivIcon (T-SDD Epica 2A). Avoids shipping a
 * custom SVG/PNG spritesheet — the emoji renders crisply at any zoom and the rotation communicates
 * heading at a glance on the live map.
 */
export function createTruckIcon(headingDeg: number): L.DivIcon {
  return L.divIcon({
    className: 'cpg-truck-marker',
    html: `<div class="cpg-truck-marker__body" style="transform: rotate(${headingDeg}deg)">🚚</div>`,
    iconSize: [32, 32],
    iconAnchor: [16, 16],
  });
}
