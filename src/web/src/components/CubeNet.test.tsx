import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { CubeNet } from './CubeNet';
import { CHALLENGE_RESULT, FRONT_PREVIEW, SOLVED } from '../test/fixtures';

describe('CubeNet', () => {
  it('renders six faces with nine stickers each', () => {
    render(<CubeNet faces={SOLVED.faces} selectedFace={null} busy={false} onSelectFace={vi.fn()} />);

    for (const face of ['Up', 'Left', 'Front', 'Right', 'Back', 'Down']) {
      expect(within(screen.getByTestId(`face-${face}`)).getAllByTestId('sticker')).toHaveLength(9);
    }
  });

  it('describes each face to screen readers row by row', () => {
    render(<CubeNet faces={CHALLENGE_RESULT.faces} selectedFace={null} busy={false} onSelectFace={vi.fn()} />);

    expect(screen.getByRole('button', { name: 'Up face' })).toHaveAccessibleDescription(
      'row 1: red, orange, green. row 2: blue, white, white. row 3: blue, blue, blue.',
    );
  });

  it('selects a face when it is clicked', async () => {
    const onSelectFace = vi.fn();
    render(<CubeNet faces={SOLVED.faces} selectedFace="Front" busy={false} onSelectFace={onSelectFace} />);

    await userEvent.click(screen.getByRole('button', { name: 'Right face' }));

    expect(onSelectFace).toHaveBeenCalledWith('Right');
    expect(screen.getByRole('button', { name: 'Front face' })).toHaveAttribute('aria-pressed', 'true');
  });

  it('ignores clicks while busy but keeps the faces focusable', async () => {
    const onSelectFace = vi.fn();
    render(<CubeNet faces={SOLVED.faces} selectedFace={null} busy onSelectFace={onSelectFace} />);

    const face = screen.getByRole('button', { name: 'Right face' });
    await userEvent.click(face);

    expect(onSelectFace).not.toHaveBeenCalled();
    expect(face).not.toBeDisabled();
  });

  it('marks the stickers that a previewed move would change', () => {
    render(
      <CubeNet faces={SOLVED.faces} previewFaces={FRONT_PREVIEW.after} selectedFace="Front" busy={false} onSelectFace={vi.fn()} />,
    );

    const changing = screen.getAllByTestId('sticker').filter((sticker) => sticker.dataset.becomes);
    expect(changing).toHaveLength(12);
    expect(screen.getByRole('button', { name: 'Right face' })).toHaveAccessibleDescription(
      'row 1: red becomes white, red, red. row 2: red becomes white, red, red. row 3: red becomes white, red, red.',
    );
  });
});
