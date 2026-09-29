import { inverse, parseMove } from './notation';

describe('notation', () => {
  it('parses a face letter with an optional suffix', () => {
    expect(parseMove('F')).toEqual({ face: 'Front', rotation: 'Clockwise' });
    expect(parseMove("R'")).toEqual({ face: 'Right', rotation: 'AntiClockwise' });
    expect(parseMove('U2')).toEqual({ face: 'Up', rotation: 'Half' });
    expect(parseMove('X')).toBeNull();
  });

  it('inverts a turn', () => {
    expect(inverse({ face: 'Front', rotation: 'Clockwise' })).toEqual({ face: 'Front', rotation: 'AntiClockwise' });
    expect(inverse({ face: 'Down', rotation: 'Half' })).toEqual({ face: 'Down', rotation: 'Half' });
  });
});
