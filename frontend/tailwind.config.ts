import type { Config } from 'tailwindcss';

/**
 * Single source of visual truth for the CPG Enterprises frontend.
 * Synced with the "Industrial Fleet & Logistics" design system
 * (cpg_freight_logistics_landing_page/*.md). The prose palette
 * (#0B192C primary) is authoritative over the prototype's inline
 * tailwind.config (#000000), per SPEC.md design alignment.
 */
const config: Config = {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Core industrial hierarchy
        primary: {
          DEFAULT: '#0B192C', // deep maritime navy
          container: '#0E1C2F',
        },
        secondary: {
          DEFAULT: '#1E293B', // dark slate steel
          container: '#D5E0F8',
        },
        tertiary: {
          DEFAULT: '#EA580C', // hazard orange - operational alerts only, never CTAs
        },
        neutral: '#64748B',

        // Named functional colors
        'safety-amber': '#F59E0B',
        'hazard-orange': '#EA580C',
        // Corporate action colour - primary CTAs and interactive accents.
        'fleet-blue': {
          DEFAULT: '#1C3766',
          hover: '#24467F',
          soft: '#E8EEF9',
        },
        'signal-red': '#DD1A1A',
        'steel-gray': '#334155',

        // Surfaces
        surface: {
          DEFAULT: '#F8FAFC',
          light: '#F8FAFC',
          muted: '#F1F5F9',
          card: '#FFFFFF',
        },
        'on-surface': '#0B1C30',
        'on-surface-variant': '#44474C',
        outline: '#CBD5E1',
        'outline-strong': '#94A3B8',

        // Status tints
        success: { DEFAULT: '#047857', container: '#ECFDF5' },
        warning: { DEFAULT: '#B45309', container: '#FEF3C7' },
        error: { DEFAULT: '#BA1A1A', container: '#FFDAD6' },
      },
      fontFamily: {
        display: ['Chivo', 'system-ui', 'sans-serif'],
        heading: ['Chivo', 'system-ui', 'sans-serif'],
        body: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', 'monospace'],
      },
      fontSize: {
        'display-lg': ['3.5rem', { lineHeight: '4rem', letterSpacing: '-0.02em', fontWeight: '800' }],
        'headline-xl': ['2.5rem', { lineHeight: '3rem', letterSpacing: '-0.015em', fontWeight: '700' }],
        'headline-lg': ['2rem', { lineHeight: '2.5rem', letterSpacing: '-0.01em', fontWeight: '700' }],
        'headline-md': ['1.5rem', { lineHeight: '2rem', fontWeight: '600' }],
        'headline-sm': ['1.25rem', { lineHeight: '1.75rem', fontWeight: '600' }],
        'body-lg': ['1.125rem', { lineHeight: '1.75rem' }],
        'body-md': ['1rem', { lineHeight: '1.5rem' }],
        'body-sm': ['0.875rem', { lineHeight: '1.25rem' }],
        'label-md': ['0.75rem', { lineHeight: '1rem', letterSpacing: '0.04em', fontWeight: '600' }],
        'label-sm': ['0.6875rem', { lineHeight: '0.875rem', letterSpacing: '0.06em', fontWeight: '500' }],
      },
      borderRadius: {
        DEFAULT: '0.5rem', // 8px - buttons & inputs
        md: '0.625rem', // 10px
        lg: '0.75rem', // 12px - cards & panels
        xl: '1rem', // 16px - large surfaces
      },
      maxWidth: {
        container: '80rem',
      },
      boxShadow: {
        // Soft, low-contrast elevation for the premium card system.
        sm: '0 1px 2px rgba(15, 23, 42, 0.04), 0 1px 3px rgba(15, 23, 42, 0.06)',
        DEFAULT: '0 1px 2px rgba(15, 23, 42, 0.05), 0 2px 6px rgba(15, 23, 42, 0.07)',
        md: '0 2px 4px rgba(15, 23, 42, 0.05), 0 6px 16px rgba(15, 23, 42, 0.08)',
        elevated:
          '0 4px 6px -1px rgba(11, 25, 44, 0.08), 0 2px 4px -2px rgba(11, 25, 44, 0.06)',
        overlay:
          '0 20px 25px -5px rgba(11, 25, 44, 0.18), 0 8px 10px -6px rgba(11, 25, 44, 0.12)',
      },
    },
  },
  plugins: [],
};

export default config;
