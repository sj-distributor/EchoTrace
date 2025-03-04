using Autofac;
using EchoTrace.Realization.Bases;
using Shouldly;

namespace EchoTrace.UnitTests;

public class AgileMapperFacts : TestBase
{
    [Fact]
    public void ShouldCanAutoMapperByPropertyName()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            TestId = Guid.NewGuid()
        };
        var testDto = agileMapper.Map<TestEntity, TestDto>(testEntity);
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBe(default);
    }

    [Fact]
    public Task ShouldCanMapperByValue()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            TestId = Guid.NewGuid()
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity,
                e => { e.Profile(x => x.Target.CreatedName, "小明"); });
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBe("小明");
        return Task.CompletedTask;
    }

    [Fact]
    public Task ShouldCanMapperBySpecificProperty()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid()
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity,
                e => { e.Profile(x => x.Target.CreatedName, x => x.Source.Creator); });
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBe(testEntity.Creator);
        return Task.CompletedTask;
    }

    [Fact]
    public Task ShouldPropertyValueBeLastMapValue()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid()
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity,
                e =>
                {
                    e.Profile(x => x.Target.CreatedName, x => x.Source.Creator)
                        .Profile(x => x.Target.CreatedName, "小白")
                        .Profile(x => x.Target.CreatedName, x => x.Value("小绿"));
                });
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBe("小绿");
        return Task.CompletedTask;
    }


    [Fact]
    public Task ShouldCanIgnoreMapPropertyValue()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid()
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity,
                e =>
                {
                    e.Profile(x => x.Target.TestName, x => x.Ignore());
                    e.Profile(x => x.Target.CreatedName, "小绿");
                });
        testDto.TestName.ShouldBeNull();
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBe("小绿");
        return Task.CompletedTask;
    }


    [Fact]
    public Task ShouldCanIgnoreMapPropertyValueWhenPropertyNameNotEqual()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid()
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity);
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBeNull();
        return Task.CompletedTask;
    }

    [Fact]
    public Task ShouldCanIgnoreMapPropertyValueWhenPropertyTypeNotEqual()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid()
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity);
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBeNull();
        testDto.Age.ShouldBeNull();
        return Task.CompletedTask;
    }

    [Fact]
    public Task ShouldCanMapWhenConvertType()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid(),
            Age = 10
        };
        var testDto =
            agileMapper.Map<TestEntity, TestDto>(testEntity,
                e => { e.Profile(x => x.Target.Age, x => x.Source.Age.ToString()); });
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBeNull();
        testDto.Age.ShouldBe(testEntity.Age.ToString());
        return Task.CompletedTask;
    }

    [Fact]
    public Task ShouldCanMapWhenTargetIsCreated()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid(),
            Age = 10
        };
        var testDto = new TestDto
        {
            TestName = "TargetTestName",
            Age = "20",
            CreatedOn = DateTimeOffset.Now.AddDays(1),
            CreatedName = "小明",
            TestId = Guid.NewGuid()
        };
        agileMapper.Map(testEntity, testDto);
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testEntity.TestId);
        testDto.CreatedName.ShouldBe("小明");
        testDto.Age.ShouldBe("20");
        return Task.CompletedTask;
    }

    [Fact]
    public Task ShouldNotMapWhenTargetIsCreatedAndSpecificIgnore()
    {
        var agileMapper = TestLifetimeScope.Resolve<IAgileMapper>();
        var testEntity = new TestEntity
        {
            TestName = "TestEntityName",
            CreatedOn = DateTimeOffset.Now,
            Creator = "小红",
            TestId = Guid.NewGuid(),
            Age = 10
        };
        var testDtoId = Guid.NewGuid();
        var testDto = new TestDto
        {
            TestName = "TargetTestName",
            Age = "20",
            CreatedOn = DateTimeOffset.Now.AddDays(1),
            CreatedName = "小明",
            TestId = testDtoId
        };
        agileMapper.Map(testEntity, testDto, e => { e.Profile(x => x.Target.TestId, x => x.Ignore()); });
        testDto.TestName.ShouldBe(testEntity.TestName);
        testDto.CreatedOn.ShouldBe(testEntity.CreatedOn);
        testDto.TestId.ShouldBe(testDtoId);
        testDto.CreatedName.ShouldBe("小明");
        testDto.Age.ShouldBe("20");
        return Task.CompletedTask;
    }

    private class TestEntity
    {
        public Guid TestId { get; set; }

        public string TestName { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string Creator { get; set; }

        public int Age { get; set; }
    }

    private class TestDto
    {
        public Guid TestId { get; set; }

        public string TestName { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string CreatedName { get; set; }

        public string Age { get; set; }
    }
}