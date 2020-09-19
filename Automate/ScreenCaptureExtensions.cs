using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace Automate {
    public static class ScreenCaptureExtensions {
        /// <summary>
        /// Search for a <see cref="Template"/> and return the first match
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="t"></param>
        /// <returns>Center point of the found match</returns>
        public static Point LocateCenter(this ScreenCapture sc, Template t) {
            int offset = t.Size / 2;
            Point p = Point.Empty;
            // t.Width - t.Size so the template won't be outside of sc
            Parallel.For(t.X, t.X + t.Width - t.Size, (x, stateX) => {
                Parallel.For(t.Y, t.Y + t.Height - t.Size, (y, stateY) => {
                    if(sc.MatchesRectangle(x + y * sc.Width, t.Size, t.Size, t.Color, t.Tolerance2)) {
                        p = new Point(x + offset, y + offset);
                        stateX.Break();
                        stateY.Break();
                    }
                });
            });
            return p;
        }

        /// <summary>
        /// Search for a <see cref="Template"/> and return all matches
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="t"></param>
        /// <returns>Center points of all found matches</returns>
        public static IEnumerable<Point> LocateCenterAll(this ScreenCapture sc, Template t) {
            List<Rectangle> covered = new List<Rectangle>();
            int offset = t.Size / 2;
            for(int x = t.X; x < t.X + t.Width - t.Size; x++) {
                for(int y = t.Y; y < t.Y + t.Height - t.Size; y++) {
                    // Skip if current point is in found region
                    if(covered.Where(rect => rect.Contains(x, y)).Any()) continue;
                    if(sc.MatchesRectangle(x + y * sc.Width, t.Size, t.Size, t.Color, t.Tolerance2)) {
                        // Add found region to covered with extended distance
                        covered.Add(new Rectangle(
                            x - t.Distance,
                            y - t.Distance,
                            t.Size + t.Distance * 2,
                            t.Size + t.Distance * 2
                        ));
                        yield return new Point(x + offset, y + offset);
                        // Skip until outside of found region
                        x += t.Size;
                    }
                }
            }
        }
    }
}
