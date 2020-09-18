using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Automate {
    public static class ScreenCaptureExtensions {
        /// <summary>
        /// Search for a <see cref="ScreenCapture"/> and return the first match
        /// </summary>
        /// <param name="tolerance">Minimum distance between 2 colors</param>
        /// <returns></returns>
        public static Point Locate(this ScreenCapture heystack, ScreenCapture needle, int tolerance = 0) {
            // tolerance squared
            tolerance *= tolerance;
            // h.Width - n.Width so the needle won't be outside of heystack ( same for GL(0) )
            for(int x1 = 0; x1 <= heystack.Width - needle.Width; x1++) {
                for(int y1 = 0; y1 <= heystack.Height - needle.Height; y1++) {
                    if(heystack.MatchesWith(x1 + y1 * heystack.Width, needle, tolerance)) {
                        // return middle point
                        return new Point(x1 + needle.Width / 2, y1 + needle.Height / 2);
                    }
                }
            }
            return Point.Empty;
        }

        /// <summary>
        /// Search for a <see cref="ScreenCapture"/> and return all the results
        /// </summary>
        /// <param name="tolerance">Maximum distance between 2 colors</param>
        /// <param name="distance">Minimun distance between 2 found areas</param>
        /// <returns></returns>
        public static IEnumerable<Point> LocateAll(this ScreenCapture heystack, ScreenCapture needle, int tolerance = 0, int distance = 0) {
            tolerance *= tolerance;
            List<Rectangle> covered = new List<Rectangle>();
            for(int x1 = 0; x1 <= heystack.Width - needle.Width; x1++) {
                for(int y1 = 0; y1 <= heystack.Height - needle.Height; y1++) {
                    // Skip pixels that are in found areas
                    if(covered.Select(rect => rect.Contains(x1, y1)).Any()) continue;
                    // Add rect and return point if matches
                    if(heystack.MatchesWith(x1 + y1 * heystack.Width, needle, tolerance)) {
                        // Add needle with minimum distance
                        covered.Add(new Rectangle(
                            x1 - distance,
                            y1 - distance,
                            needle.Width + distance * 2,
                            needle.Height + distance * 2
                        ));

                        yield return new Point(x1 + needle.Width / 2, y1 + needle.Height / 2);
                        // continue outside the needle
                        x1 += needle.Width;
                    }
                }
            }
        }

        /// <summary>
        /// Search for a <see cref="Template"/> and return the first match
        /// </summary>
        /// <param name="heystack"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Point LocateTemplate(this ScreenCapture heystack, Template t) {
            for(int x1 = t.X; x1 <= t.X + t.Width; x1++) {
                for(int y1 = t.Y; y1 <= t.Y + t.Height; y1++) {
                    if(heystack.MatchesWith(x1 + y1 * heystack.Width, t.Color, t.Size * t.Size, t.Tolerance2)) {
                        int offset = t.Size / 2;
                        return new Point(x1 + offset, y1 + offset);
                    }
                }
            }
            return Point.Empty;
        }

        public static IEnumerable<Point> LocateTemplateAll(this ScreenCapture heystack, Template t) {
            List<Rectangle> covered = new List<Rectangle>();
            int offset = t.Size / 2;
            for(int x1 = t.X; x1 <= t.X + t.Width; x1++) {
                for(int y1 = t.Y; y1 <= t.Y + t.Height; y1++) {
                    if(covered.Where(rect => rect.Contains(x1, y1)).Any()) continue;
                    if(heystack.MatchesWith(x1 + y1 * heystack.Width, t.Color, t.Size * t.Size, t.Tolerance2)) {
                        covered.Add(new Rectangle(
                            x1 - t.Distance,
                            y1 - t.Distance,
                            t.Size + t.Distance * 2,
                            t.Size + t.Distance * 2
                        ));
                        yield return new Point(x1 + offset, y1 + offset);
                        x1 += t.Size;
                    }
                }
            }
        }
    }
}
