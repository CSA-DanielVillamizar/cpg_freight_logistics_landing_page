import { extendTailwindMerge } from 'tailwind-merge';

/**
 * `tailwind-merge` so a `className` override actually wins over a component's own
 * classes (last conflicting utility wins), instead of leaving both on the element
 * and letting CSS source order decide. Extended with this project's custom
 * `fontSize`, `fontFamily` and `boxShadow` scales from `tailwind.config.ts` so
 * e.g. `text-body-sm` is treated as a size, not mistaken for a text colour.
 */
const twMerge = extendTailwindMerge({
  extend: {
    classGroups: {
      'font-size': [
        {
          text: [
            'display-lg',
            'headline-xl',
            'headline-lg',
            'headline-md',
            'headline-sm',
            'body-lg',
            'body-md',
            'body-sm',
            'label-md',
            'label-sm',
          ],
        },
      ],
      'font-family': [{ font: ['display', 'heading', 'body'] }],
      shadow: [{ shadow: ['elevated', 'overlay'] }],
    },
  },
});

/** Join class names and resolve Tailwind conflicts. Falsy values are dropped. */
export function cn(...values: (string | false | null | undefined)[]): string {
  return twMerge(values.filter((value): value is string => Boolean(value)).join(' '));
}
