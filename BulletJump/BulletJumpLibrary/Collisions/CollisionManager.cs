using BulletJumpLibrary.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletJumpLibrary.Collisions
{
    public interface ICollidable
    {
        Rectangle GetBounds();
        Vector2 GetPosition();
        void SetPosition(Vector2 position);
    }

    public interface IPlayerCollidable : ICollidable
    {
        void SetGrounded(bool grounded);
        void SetVerticalVelocity(float velocity);
        Vector2 Velocity { get; }
        Vector2 GetColliderOffset();
        Vector2 GetColliderSize();
    }

    public interface IBulletCollidable : ICollidable
    {
        bool IsExpired { get; set; }
    }

    public static class CollisionManager
    {
        public static void HandlePlayerCollision(IPlayerCollidable player, Tilemap tilemap)
        {
            if (player == null || tilemap == null) return;

            Rectangle playerBounds = player.GetBounds();
            Point playerTile = Core.WorldToTile(player.GetPosition(), tilemap.TileWidth, tilemap.TileHeight);

            bool wasGroundedThisFrame = false;
            bool hadAnyCollision = false;

            // Проверяем коллизии со всеми окружающими тайлами
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Point checkTile = new Point(playerTile.X + x, playerTile.Y + y);

                    if (Core.IsInTilemapBounds(checkTile, tilemap.Columns, tilemap.Rows))
                    {
                        if (!tilemap.IsTileEmpty("Collision", checkTile.X, checkTile.Y))
                        {
                            hadAnyCollision = true;
                            if (HandleTileCollision(player, tilemap, checkTile))
                            {
                                wasGroundedThisFrame = true;
                            }
                        }
                    }
                }
            }

            // Если не было обычных коллизий с землей, но игрок движется вниз - проверяем землю под ногами
            if (!wasGroundedThisFrame && player.Velocity.Y >= 0)
            {
                wasGroundedThisFrame = CheckGroundBellow(player, tilemap);
            }

            // Стабильно устанавливаем состояние grounded
            player.SetGrounded(wasGroundedThisFrame);
        }

        public static void HandleBulletCollision(IEnumerable<IBulletCollidable> bullets, Tilemap tilemap)
        {
            if (bullets == null || tilemap == null) return;

            foreach (var bullet in bullets.ToList())
            {
                if (bullet.IsExpired) continue;

                // 1. Проверка столкновения со стенами
                if (CheckBulletTileCollision(bullet, tilemap))
                {
                    bullet.IsExpired = true;
                    continue;
                }

                // 2. Проверка выхода за границы уровня (опционально)
                if (CheckBulletOutOfBounds(bullet, tilemap))
                {
                    bullet.IsExpired = true;
                }

                // 3. Здесь можно добавить проверку с другими объектами
                // if (CheckBulletEnemyCollision(bullet)) ...
            }
        }

        private static bool CheckGroundBellow(IPlayerCollidable player, Tilemap tilemap)
        {
            Rectangle bounds = player.GetBounds();

            // Проверяем несколько точек под ногами для большей стабильности
            Vector2[] checkPoints = {
                new Vector2(bounds.Left + 5, bounds.Bottom + 2),
                new Vector2(bounds.Center.X, bounds.Bottom + 2),
                new Vector2(bounds.Right - 5, bounds.Bottom + 2)
            };

            bool foundGround = false;

            foreach (Vector2 checkPoint in checkPoints)
            {
                Point tileBelow = Core.WorldToTile(checkPoint, tilemap.TileWidth, tilemap.TileHeight);

                if (Core.IsInTilemapBounds(tileBelow, tilemap.Columns, tilemap.Rows))
                {
                    if (!tilemap.IsTileEmpty("Collision", tileBelow.X, tileBelow.Y))
                    {
                        foundGround = true;

                        // Корректируем позицию только если действительно нужно
                        if (ShouldCorrectPosition(player, tilemap, tileBelow))
                        {
                            Vector2 tileWorldPos = Core.TileToWorld(tileBelow, tilemap.TileWidth, tilemap.TileHeight);
                            float groundY = tileWorldPos.Y - player.GetColliderSize().Y - player.GetColliderOffset().Y;
                            player.SetPosition(new Vector2(player.GetPosition().X, groundY));
                        }
                        return true;
                    }
                    
                }
            }
            return false;
        }

        private static bool ShouldCorrectPosition(IPlayerCollidable player, Tilemap tilemap, Point tileBellow)
        {
            // Корректируем позицию только если игрок действительно падает на платформу
            Rectangle bounds = player.GetBounds();
            Vector2 tileWorldPos = Core.TileToWorld(tileBellow, tilemap.TileWidth, tilemap.TileHeight);
            float distanceToGround = tileWorldPos.Y - bounds.Bottom;

            // Корректируем только если расстояние небольшое (игрок близко к земле)
            return distanceToGround >= 0 && distanceToGround < 10f;
        }

        private static bool HandleTileCollision(IPlayerCollidable player, Tilemap tilemap, Point collisionTile)
        {
            Vector2 tileWorldPos = Core.TileToWorld(collisionTile, tilemap.TileWidth, tilemap.TileHeight);
            Rectangle tileBounds = new Rectangle(
                (int)tileWorldPos.X,
                (int)tileWorldPos.Y,
                (int)tilemap.TileWidth,
                (int)tilemap.TileHeight
            );

            Rectangle playerBounds = player.GetBounds();

            if (!playerBounds.Intersects(tileBounds))
                return false;

            Vector2 correction = CalculateCollisionCorrection(playerBounds, tileBounds);

            // Применяем коррекцию позиции игрока
            Vector2 newPosition = player.GetPosition() + correction;
            player.SetPosition(newPosition);

            // Определяем тип столкновения
            if (correction.Y < 0) // Столкновение сверху (игрок стоит на тайле)
            {
                player.SetVerticalVelocity(0);
                return true; // Это коллизия с землей
            }
            else if (correction.Y > 0) // Столкновение снизу (удар головой)
            {
                player.SetVerticalVelocity(0);
            }

            return false;

        }

        private static Vector2 CalculateCollisionCorrection(Rectangle player, Rectangle tile)
        {
            Vector2 correction = Vector2.Zero;

            // Рассчитываем глубину проникновения с каждой стороны
            float overlapLeft = player.Right - tile.Left;
            float overlapRight = tile.Right - player.Left;
            float overlapTop = player.Bottom - tile.Top;
            float overlapBottom = tile.Bottom - player.Top;

            // Находим минимальное перекрытие
            float minOverlap = float.MaxValue;
            Vector2 minCorrection = Vector2.Zero;

            if (overlapLeft > 0 && overlapLeft < minOverlap)
            {
                minOverlap = overlapLeft;
                minCorrection = new Vector2(-overlapLeft, 0);
            }

            if (overlapRight > 0 && overlapRight < minOverlap)
            {
                minOverlap = overlapRight;
                minCorrection = new Vector2(overlapRight, 0);
            }

            if (overlapTop > 0 && overlapTop < minOverlap)
            {
                minOverlap = overlapTop;
                minCorrection = new Vector2(0, -overlapTop);
            }

            if (overlapBottom > 0 && overlapBottom < minOverlap)
            {
                minOverlap = overlapBottom;
                minCorrection = new Vector2(0, overlapBottom);
            }

            return minCorrection;
        }

        private static bool CheckBulletTileCollision(IBulletCollidable bullet, Tilemap tilemap)
        {
            Rectangle bulletBounds = bullet.GetBounds();

            // Преобразуем границы пули в тайловые координаты
            Point topLeft = Core.WorldToTile(
                new Vector2(bulletBounds.Left, bulletBounds.Top),
                tilemap.TileWidth, tilemap.TileHeight);

            Point bottomRight = Core.WorldToTile(
                new Vector2(bulletBounds.Right, bulletBounds.Bottom),
                tilemap.TileWidth, tilemap.TileHeight);

            // Проверяем все тайлы в прямоугольнике пули
            for (int x = topLeft.X; x <= bottomRight.X; x++)
            {
                for (int y = topLeft.Y; y <= bottomRight.Y; y++)
                {
                    if (Core.IsInTilemapBounds(new Point(x, y), tilemap.Columns, tilemap.Rows))
                    {
                        if (!tilemap.IsTileEmpty("Collision", x, y))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool CheckBulletOutOfBounds(IBulletCollidable bullet, Tilemap tilemap)
        {
            // Проверяем, вышла ли пуля за границы уровня
            Rectangle bulletBounds = bullet.GetBounds();
            Rectangle levelBounds = new Rectangle(
                0, 0,
                (int)(tilemap.Columns * tilemap.TileWidth),
                (int)(tilemap.Rows * tilemap.TileHeight));

            return !levelBounds.Contains(bulletBounds);
        }
    }
}
