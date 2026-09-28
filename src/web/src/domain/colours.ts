import type { Colour } from './types';

/** Display colours chosen to match rubiks-cube-solver.com. */
export const COLOUR_HEX: Readonly<Record<Colour, string>> = {
  White: '#ffffff',
  Orange: '#ff9f1a',
  Green: '#4caf50',
  Red: '#e53935',
  Blue: '#2962ff',
  Yellow: '#ffeb3b',
};

export const COLOUR_SYMBOL: Readonly<Record<Colour, string>> = {
  White: 'W',
  Orange: 'O',
  Green: 'G',
  Red: 'R',
  Blue: 'B',
  Yellow: 'Y',
};
