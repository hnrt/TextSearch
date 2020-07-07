package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.argThat;
import static org.mockito.Mockito.doNothing;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import java.util.ArrayList;
import java.util.List;
import javax.persistence.EntityManager;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.aspect.RequestContext;
import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.entity.UserEntity;
import com.hideakin.textsearch.index.exception.InvalidParameterException;
import com.hideakin.textsearch.index.model.FileGroupInfo;
import com.hideakin.textsearch.index.model.UserInfo;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@SpringBootTest
public class FileGroupServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupRepository fileGroupRepository;
	
	@Mock
	private FileRepository fileRepository;
	
	@Mock
	private PreferenceRepository preferenceRepository;

	@InjectMocks
	private FileGroupService fileGroupService = new FileGroupServiceImpl();

	@Test
	public void getGroups_successful() {
		List<FileGroupEntity> entities = new ArrayList<FileGroupEntity>();
		add(entities, 0, "default");
		add(entities, 1, "foo");
		add(entities, 5, "bar");
		add(entities, 19, "baz");
		when(fileGroupRepository.findAll()).thenReturn(entities);
		FileGroupInfo[] groups = fileGroupService.getGroups();
		Assertions.assertEquals(4, groups.length);
		Assertions.assertEquals(entities.get(0).getName(), groups[0].getName());
		Assertions.assertEquals(entities.get(1).getName(), groups[1].getName());
		Assertions.assertEquals(entities.get(2).getName(), groups[2].getName());
		Assertions.assertEquals(entities.get(3).getName(), groups[3].getName());
	}

	@Test
	public void getGroups_null() {
		when(fileGroupRepository.findAll()).thenReturn(new ArrayList<FileGroupEntity>());
		FileGroupInfo[] groups = fileGroupService.getGroups();
		Assertions.assertEquals(0, groups.length);
	}

	@Test
	public void createGroup_successful1() {
		when(preferenceRepository.findByName("GID.next")).thenReturn(null);
		when(preferenceRepository.save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("791")))).thenReturn(new PreferenceEntity("GID.next", "791"));
		when(em.createQuery("SELECT MAX(gid) FROM file_groups")).thenReturn(new PseudoQuery(789));
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 790))).thenReturn(new FileGroupEntity(790, "def"));
		FileGroupInfo groupInfo = fileGroupService.createGroup("def");
		Assertions.assertEquals(790, groupInfo.getGid());
		verify(preferenceRepository, times(1)).findByName("GID.next");
		verify(em, times(1)).createQuery("SELECT MAX(gid) FROM file_groups");
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("791")));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 790));
	}

	@Test
	public void createGroup_successful2() {
		when(preferenceRepository.findByName("GID.next")).thenReturn(new PreferenceEntity("GID.next", "791"));
		when(em.createQuery("SELECT MAX(gid) FROM file_groups")).thenReturn(new PseudoQuery(700));
		when(preferenceRepository.save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("792")))).thenReturn(new PreferenceEntity("GID.next", "792"));
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 791))).thenReturn(new FileGroupEntity(791, "ghi"));
		FileGroupInfo groupInfo = fileGroupService.createGroup("ghi");
		Assertions.assertEquals(791, groupInfo.getGid());
		verify(preferenceRepository, times(1)).findByName("GID.next");
		verify(em, times(0)).createQuery("SELECT MAX(gid) FROM file_groups");
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("792")));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 791));
	}

	@Test
	public void createGroup_invalidName() {
		when(preferenceRepository.findByName("GID.next")).thenReturn(new PreferenceEntity("GID.next", "791"));
		when(em.createQuery("SELECT MAX(gid) FROM file_groups")).thenReturn(new PseudoQuery(700));
		when(preferenceRepository.save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("792")))).thenReturn(new PreferenceEntity("GID.next", "792"));
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 791))).thenReturn(new FileGroupEntity(791, "ghi"));
		InvalidParameterException exception = Assertions.assertThrows(InvalidParameterException.class, () -> {
			fileGroupService.createGroup("123");
		});
		Assertions.assertEquals("Invalid group name.", exception.getMessage());
		verify(preferenceRepository, times(0)).findByName("GID.next");
		verify(em, times(0)).createQuery("SELECT MAX(gid) FROM file_groups");
		verify(preferenceRepository, times(0)).save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("792")));
		verify(fileGroupRepository, times(0)).save(argThat(x -> x.getGid() == 800));
	}

	@Test
	public void updateGroup_successful() {
		when(fileGroupRepository.findByGid(234)).thenReturn(new FileGroupEntity(234, "ghh"));
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 234))).thenReturn(new FileGroupEntity(234, "ghi"));
		FileGroupInfo groupInfo = fileGroupService.updateGroup(234, "ghi");
		Assertions.assertEquals(234, groupInfo.getGid());
		verify(fileGroupRepository, times(1)).findByGid(234);
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 234));
	}

	@Test
	public void updateGroup_notFound() {
		when(fileGroupRepository.findByGid(345)).thenReturn(null);
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 345))).thenReturn(null);
		FileGroupInfo groupInfo = fileGroupService.updateGroup(345, "ghi");
		Assertions.assertEquals(null, groupInfo);
		verify(fileGroupRepository, times(1)).findByGid(345);
		verify(fileGroupRepository, times(0)).save(argThat(x -> x.getGid() == 345));
	}

	@Test
	public void delete_successful() {
		UserEntity ue = new UserEntity();
		ue.setUsername("anonymous");
		ue.setRoles("administrator");
		RequestContext.setUserInfo(new UserInfo(ue));
		when(fileGroupRepository.findByGid(567)).thenReturn(new FileGroupEntity(567, "xyzzy"));
		when(fileRepository.findAllByGid(567)).thenReturn(new ArrayList<FileEntity>());
		doNothing().when(fileRepository).delete(argThat(x -> x.getGid() == 567));
		FileGroupInfo groupInfo = fileGroupService.deleteGroup(567);
		Assertions.assertEquals(567, groupInfo.getGid());
		Assertions.assertEquals("xyzzy", groupInfo.getName());
		verify(fileGroupRepository, times(1)).findByGid(567);
		verify(fileRepository, times(1)).findAllByGid(567);
		verify(fileGroupRepository, times(1)).delete(argThat(x -> x.getGid() == 567));
	}

	@Test
	public void delete_notFound() {
		when(fileGroupRepository.findByGid(567)).thenReturn(null);
		when(fileRepository.findAllByGid(567)).thenReturn(new ArrayList<FileEntity>());
		doNothing().when(fileRepository).delete(argThat(x -> x.getGid() == 567));
		FileGroupInfo groupInfo = fileGroupService.deleteGroup(567);
		Assertions.assertEquals(null, groupInfo);
		verify(fileGroupRepository, times(1)).findByGid(567);
		verify(fileRepository, times(0)).findAllByGid(567);
		verify(fileGroupRepository, times(0)).delete(argThat(x -> x.getGid() == 567));
	}

	private void add(List<FileGroupEntity> entities, int gid, String name) {
		entities.add(new FileGroupEntity(gid, name));
	}

}
