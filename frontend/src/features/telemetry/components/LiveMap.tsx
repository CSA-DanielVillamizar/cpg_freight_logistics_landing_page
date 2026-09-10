import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import { useEffect, useRef } from 'react';
import { createTruckIcon } from './TruckMarker';

interface LiveMapProps {
    latitude: number;
    longitude: number;
    headingDeg?: number;
    /** GPS trail, oldest first, as `[lat, lng]` pairs. */
    trail?: [number, number][];
    className?: string;
}

/**
 * Real interactive map (Leaflet + OpenStreetMap tiles — no API key, no Google Maps licensing)
 * showing the truck's live position and recent trail (T-SDD Epica 2A). Leaflet owns the DOM
 * node imperatively; React only re-renders the container `div` and pushes prop changes in.
 */
export function LiveMap({
    latitude,
    longitude,
    headingDeg = 0,
    trail = [],
    className,
}: LiveMapProps): JSX.Element {
    const containerRef = useRef<HTMLDivElement>(null);
    const mapRef = useRef<L.Map | null>(null);
    const markerRef = useRef<L.Marker | null>(null);
    const trailRef = useRef<L.Polyline | null>(null);

    // Mount once: create the map, tile layer, marker and trail polyline.
    useEffect(() => {
        if (!containerRef.current || mapRef.current) {
            return undefined;
        }

        const map = L.map(containerRef.current, { attributionControl: true }).setView(
            [latitude, longitude],
            8,
        );

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 18,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        }).addTo(map);

        trailRef.current = L.polyline([], { color: '#1C3766', weight: 3, opacity: 0.7 }).addTo(map);
        markerRef.current = L.marker([latitude, longitude], { icon: createTruckIcon(headingDeg) }).addTo(map);
        mapRef.current = map;

        return () => {
            map.remove();
            mapRef.current = null;
            markerRef.current = null;
            trailRef.current = null;
        };
        // Mount/unmount only — subsequent prop changes are applied by the effects below.
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // Move the marker + recenter whenever a new GPS reading arrives.
    useEffect(() => {
        const marker = markerRef.current;
        const map = mapRef.current;
        if (!marker || !map) {
            return;
        }
        marker.setLatLng([latitude, longitude]);
        marker.setIcon(createTruckIcon(headingDeg));
        map.panTo([latitude, longitude], { animate: true });
    }, [latitude, longitude, headingDeg]);

    // Redraw the trail when it grows.
    useEffect(() => {
        trailRef.current?.setLatLngs(trail);
    }, [trail]);

    return (
        <div
            ref={containerRef}
            className={className ?? 'h-full w-full'}
            role="img"
            aria-label="Live truck location map"
        />
    );
}
