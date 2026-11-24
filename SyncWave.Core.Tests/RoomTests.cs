using SyncWave.Core.Entities;
using SyncWave.Core.Enums;

namespace SyncWave.Core.Tests;

/// <summary>
/// Тестовий клас для перевірки функціональності класу Room.
/// </summary>
public class RoomTests
{
    // Тест для перевірки додавання першого учасника до кімнати та встановлення його як хоста
    [Fact]
    public void AddParticipant_WhenFirstParticipant_ShouldSetAsHost()
    {
        // Arrange
        // Створюємо нову кімнату та учасника
        var room = new Room { RoomCode = "Test-Room" };
        var participant = new Participant { ConnectionId = "Test-Connection" , Username = "Test-User" };

        // Act
        // Додаємо учасника до кімнати
        room.AddParticipant(participant);

        // Assert
        // Перевіряємо, що учасник встановлений як хост та що він єдиний учасник у кімнаті
        Assert.True(participant.IsHost);
        Assert.Single(room.Participants);

    }

    // Тест для перевірки додавання другого учасника до кімнати та перевірки, що він не встановлений як хост
    [Fact]
    public void AddParticipant_WhenSecondParticipant_ShouldNotSetAsHost()
    {
        // Arrange
        // Створюємо нову кімнату та двох учасників
        var room = new Room { RoomCode = "Test-Room" };
        var host = new Participant { ConnectionId = "Host-Connection", Username = "Host-User" };
        var guest = new Participant { ConnectionId = "Guest-Connection", Username = "Guest-User" };
        room.AddParticipant(host); // Додаємо першого учасника до кімнати

        // Act
        // Додаємо другого учасника до кімнати
        room.AddParticipant(guest);
       
        // Assert
        // Перевіряємо, що перший учасник встановлений як хост, а другий - ні, та що обидва учасники присутні у кімнаті
        Assert.True(host.IsHost);
        Assert.False(guest.IsHost);
        Assert.Equal(2, room.Participants.Count);
    }

    // Тест для перевірки видалення хоста з кімнати та передачі ролі хоста наступному учаснику
    [Fact]
    public void RemoveParticipant_WhenHostLeaves_ShouldTransferHostToNextParticipant()
    {
        // Arrange
        // Створюємо нову кімнату та двох учасників
        var room = new Room { RoomCode = "Test-Room" };
        var host = new Participant { ConnectionId = "Host-Connection", Username = "Host-User" };
        var guest = new Participant { ConnectionId = "Guest-Connection", Username = "Guest-User" };
        room.AddParticipant(host); // Додаємо першого учасника до кімнати
        room.AddParticipant(guest); // Додаємо другого учасника до кімнати

        // Act
        // Видаляємо хоста з кімнати
        room.RemoveParticipant(host.ConnectionId);

        // Assert
        // Перевіряємо, що другий учасник тепер є хостом та що в кімнаті залишився лише один учасник
        Assert.True(guest.IsHost);
        Assert.Single(room.Participants);
    }

    // Тест для перевірки видалення останнього учасника з кімнати: кімната має повністю скинутися
    [Fact]
    public void RemoveParticipant_WhenLastParticipantLeaves_ShouldResetRoom()
    {
        // Arrange
        // Створюємо кімнату з одним учасником, у якій грає медіа і ще одне чекає в черзі
        var room = new Room { RoomCode = "Test-Room" };
        var participant = new Participant { ConnectionId = "Test-Connection", Username = "Test-User" };
        room.AddParticipant(participant);
        room.EnqueueMedia(new MediaItem { Title = "Song 1", Duration = TimeSpan.FromMinutes(3) });
        room.EnqueueMedia(new MediaItem { Title = "Song 2", Duration = TimeSpan.FromMinutes(4) });
        room.SeekTo(TimeSpan.FromSeconds(30));

        // Act
        // Видаляємо останнього учасника з кімнати
        room.RemoveParticipant(participant.ConnectionId);

        // Assert
        // Перевіряємо, що в кімнаті нікого немає, відтворення зупинене, медіа, позиція і черга скинуті
        Assert.Empty(room.Participants);
        Assert.Equal(PlaybackState.Stopped, room.State);
        Assert.Null(room.CurrentMedia);
        Assert.Equal(TimeSpan.Zero, room.CurrentPosition);
        Assert.Empty(room.Queue);
    }

    // Регресійний тест: після того як кімната спорожніла, нове медіа має одразу почати грати
    [Fact]
    public void EnqueueMedia_AfterRoomBecameEmpty_ShouldStartPlayingImmediately()
    {
        // Arrange
        // Учасник додав медіа, потім вийшов; заходить новий учасник
        var room = new Room { RoomCode = "Test-Room" };
        var first = new Participant { ConnectionId = "First-Connection", Username = "First-User" };
        room.AddParticipant(first);
        room.EnqueueMedia(new MediaItem { Title = "Old Song", Duration = TimeSpan.FromMinutes(3) });
        room.RemoveParticipant(first.ConnectionId);
        room.AddParticipant(new Participant { ConnectionId = "Second-Connection", Username = "Second-User" });
        var newMedia = new MediaItem { Title = "New Song", Duration = TimeSpan.FromMinutes(2) };

        // Act
        // Новий учасник додає медіа
        room.EnqueueMedia(newMedia);

        // Assert
        // Нове медіа має стати поточним і грати, а не опинитися в черзі
        Assert.Equal(newMedia, room.CurrentMedia);
        Assert.Equal(PlaybackState.Playing, room.State);
        Assert.Empty(room.Queue);
    }

    // Тест для перевірки додавання медіа-елемента до черги відтворення, коли нічого не відтворюється
    [Fact]
    public void EnqueueMedia_WhenNoMediaPlaying_ShouldStartPlayingImmediately()
    {
        // Arrange
        // Створюємо нову кімнату та медіа-елемент
        var room = new Room { RoomCode = "Test-Room" };
        var media = new MediaItem { Title = "Song 1", SourceUrl = "http://testmedia.com/song1", Duration = TimeSpan.FromMinutes(3) };

        // Act
        // Додаємо медіа-елемент до черги відтворення
        room.EnqueueMedia(media);

        // Assert
        // Перевіряємо, що медіа-елемент встановлений як поточний та що стан відтворення встановлений на "Playing"
        Assert.NotNull(room.CurrentMedia);
        Assert.Equal(media, room.CurrentMedia);
        Assert.Equal(PlaybackState.Playing, room.State);
        Assert.Equal(TimeSpan.Zero, room.CurrentPosition);
        Assert.Empty(room.Queue);
    }

    // Тест для перевірки додавання медіа-елемента до черги відтворення, коли вже відтворюється інший медіа-елемент
    [Fact]
    public void EnqueueMedia_WhenMediaAlreadyPlaying_ShouldAddToQueue()
    {
        // Arrange
        // Створюємо нову кімнату та два медіа-елементи
        var room = new Room { RoomCode = "Test-Room" };
        var media1 = new MediaItem { Title = "Song 1", SourceUrl = "http://testmedia.com/song1", Duration = TimeSpan.FromMinutes(3) };
        var media2 = new MediaItem { Title = "Song 2", SourceUrl = "http://testmedia.com/song2", Duration = TimeSpan.FromMinutes(4) };
        room.EnqueueMedia(media1); // Додаємо перший медіа-елемент до черги відтворення

        // Act
        // Додаємо другий медіа-елемент до черги відтворення
        room.EnqueueMedia(media2);

        // Assert
        // Перевіряємо, що перший медіа-елемент встановлений як поточний, а другий доданий до черги
        Assert.Equal(media1, room.CurrentMedia);
        Assert.Single(room.Queue);
        Assert.Equal(media2, room.Queue.First());
    }

    // Тест для перевірки відтворення наступного медіа-елемента з черги та видалення його з черги
    [Fact]
    public void PlayNext_WhenQueueHasItems_ShouldPlayNextAndRemoveFromQueue()
    {

        // Arrange
        // Створюємо нову кімнату та два медіа-елементи
        var room = new Room { RoomCode = "Test-Room" };
        var media1 = new MediaItem { Title = "Song 1", SourceUrl = "http://testmedia.com/song1", Duration = TimeSpan.FromMinutes(3) };
        var media2 = new MediaItem { Title = "Song 2", SourceUrl = "http://testmedia.com/song2", Duration = TimeSpan.FromMinutes(4) };
        room.EnqueueMedia(media1);
        room.EnqueueMedia(media2);

        // Act
        // Викликаємо метод для відтворення наступного медіа-елемента
        room.PlayNext();

        // Assert
        // Перевіряємо, що другий медіа-елемент встановлений як поточний, черга порожня,
        // стан відтворення встановлений на "Playing" та позиція відтворення скинута
        Assert.Equal(media2, room.CurrentMedia);
        Assert.Empty(room.Queue);
        Assert.Equal(PlaybackState.Playing, room.State);
        Assert.Equal(TimeSpan.Zero, room.CurrentPosition);

    }

    // Тест для перевірки відтворення наступного медіа-елемента, коли черга порожня, та перевірки, що відтворення зупиняється
    [Fact]
    public void PlayNext_WhenQueueIsEmpty_ShouldStopPlaybackAndClearCurrentMedia()
    {
        // Arrange
        // Створюємо нову кімнату та медіа-елемент
        var room = new Room { RoomCode = "Test-Room" };
        var media = new MediaItem { Title = "Song 1", SourceUrl = "http://testmedia.com/song1", Duration = TimeSpan.FromMinutes(3) };
        room.EnqueueMedia(media);

        // Act
        // Викликаємо метод для відтворення наступного медіа-елемента, коли черга порожня
        room.PlayNext();

        // Assert
        // Перевіряємо, що поточний медіа-елемент відсутній, стан відтворення встановлений на "Stopped" та позиція відтворення скинута
        Assert.Null(room.CurrentMedia);
        Assert.Equal(PlaybackState.Stopped, room.State);
        Assert.Equal(TimeSpan.Zero, room.CurrentPosition);
    }

    // Тест для перевірки паузи під час відтворення
    [Fact]
    public void Pause_WhenPlaying_ShouldSetPaused()
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };
        room.EnqueueMedia(new MediaItem { Title = "Song 1", Duration = TimeSpan.FromMinutes(3) });

        // Act
        room.Pause();

        // Assert
        Assert.Equal(PlaybackState.Paused, room.State);
    }

    // Тест для перевірки, що пауза нічого не змінює, коли нічого не грає
    [Fact]
    public void Pause_WhenStopped_ShouldStayStopped()
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };

        // Act
        room.Pause();

        // Assert
        Assert.Equal(PlaybackState.Stopped, room.State);
    }

    // Тест для перевірки відновлення відтворення після паузи
    [Fact]
    public void Resume_WhenPaused_ShouldSetPlaying()
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };
        room.EnqueueMedia(new MediaItem { Title = "Song 1", Duration = TimeSpan.FromMinutes(3) });
        room.Pause();

        // Act
        room.Resume();

        // Assert
        Assert.Equal(PlaybackState.Playing, room.State);
    }

    // Тест для перевірки, що Resume без медіа не запускає відтворення
    [Fact]
    public void Resume_WhenNoMedia_ShouldStayStopped()
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };

        // Act
        room.Resume();

        // Assert
        Assert.Equal(PlaybackState.Stopped, room.State);
    }

    // Тест для перевірки перемотування в межах тривалості медіа (включно з самим кінцем)
    [Theory]
    [InlineData(0)]
    [InlineData(90)]
    [InlineData(180)] // рівно кінець треку — межа включна
    public void SeekTo_WhenPositionWithinDuration_ShouldUpdatePosition(int seconds)
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };
        room.EnqueueMedia(new MediaItem { Title = "Song 1", Duration = TimeSpan.FromMinutes(3) });
        var position = TimeSpan.FromSeconds(seconds);

        // Act
        room.SeekTo(position);

        // Assert
        Assert.Equal(position, room.CurrentPosition);
    }

    // Тест для перевірки, що позиція за межами медіа ігнорується
    [Theory]
    [InlineData(-1)]  // від'ємна позиція
    [InlineData(181)] // за кінцем треку
    public void SeekTo_WhenPositionOutOfRange_ShouldKeepCurrentPosition(int seconds)
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };
        room.EnqueueMedia(new MediaItem { Title = "Song 1", Duration = TimeSpan.FromMinutes(3) });
        room.SeekTo(TimeSpan.FromSeconds(30));

        // Act
        room.SeekTo(TimeSpan.FromSeconds(seconds));

        // Assert
        Assert.Equal(TimeSpan.FromSeconds(30), room.CurrentPosition);
    }

    // Тест для перевірки, що перемотування без медіа нічого не робить
    [Fact]
    public void SeekTo_WhenNoMedia_ShouldKeepZeroPosition()
    {
        // Arrange
        var room = new Room { RoomCode = "Test-Room" };

        // Act
        room.SeekTo(TimeSpan.FromSeconds(10));

        // Assert
        Assert.Equal(TimeSpan.Zero, room.CurrentPosition);
    }

}
