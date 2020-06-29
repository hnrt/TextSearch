package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.argThat;
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

import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.PreferenceEntity;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.PreferenceRepository;

@SpringBootTest
public class FileGroupServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupRepository fileGroupRepository;
	
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
		String[] groups = fileGroupService.getGroups();
		Assertions.assertEquals(4, groups.length);
		Assertions.assertEquals(entities.get(0).getName(), groups[0]);
		Assertions.assertEquals(entities.get(1).getName(), groups[1]);
		Assertions.assertEquals(entities.get(2).getName(), groups[2]);
		Assertions.assertEquals(entities.get(3).getName(), groups[3]);
	}

	@Test
	public void getGroups_null() {
		when(fileGroupRepository.findAll()).thenReturn(null);
		String[] groups = fileGroupService.getGroups();
		Assertions.assertEquals(0, groups.length);
	}

	@Test
	public void getGid_successful() {
		when(fileGroupRepository.findByName("abc")).thenReturn(new FileGroupEntity(123, "abc", "root"));
		int gid = fileGroupService.getGid("abc");
		Assertions.assertEquals(123, gid);
	}

	@Test
	public void getGid_null() {
		when(fileGroupRepository.findByName("xyz")).thenReturn(null);
		int gid = fileGroupService.getGid("xyz");
		Assertions.assertEquals(-1, gid);
	}

	@Test
	public void addGroup_successful1() {
		when(preferenceRepository.findByName("GID.next")).thenReturn(null);
		when(preferenceRepository.save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("791")))).thenReturn(new PreferenceEntity("GID.next", "791"));
		when(em.createQuery("SELECT MAX(gid) FROM file_groups")).thenReturn(new PseudoQuery(789));
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 790))).thenReturn(new FileGroupEntity(790, "def", "root"));
		int gid = fileGroupService.addGroup("def");
		Assertions.assertEquals(790, gid);
		verify(preferenceRepository, times(1)).findByName("GID.next");
		verify(em, times(1)).createQuery("SELECT MAX(gid) FROM file_groups");
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("791")));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 790));
	}

	@Test
	public void addGroup_successful2() {
		when(preferenceRepository.findByName("GID.next")).thenReturn(new PreferenceEntity("GID.next", "791"));
		when(em.createQuery("SELECT MAX(gid) FROM file_groups")).thenReturn(new PseudoQuery(700));
		when(preferenceRepository.save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("792")))).thenReturn(new PreferenceEntity("GID.next", "792"));
		when(fileGroupRepository.save(argThat(x -> x.getGid() == 791))).thenReturn(new FileGroupEntity(791, "ghi", "root"));
		int gid = fileGroupService.addGroup("ghi");
		Assertions.assertEquals(791, gid);
		verify(preferenceRepository, times(1)).findByName("GID.next");
		verify(em, times(0)).createQuery("SELECT MAX(gid) FROM file_groups");
		verify(preferenceRepository, times(1)).save(argThat(x -> x.getName().equals("GID.next") && x.getValue().equals("792")));
		verify(fileGroupRepository, times(1)).save(argThat(x -> x.getGid() == 791));
	}

	@Test
	public void delete_successful() {
		int gid = 567;
		fileGroupService.delete(gid);
		verify(fileGroupRepository, times(1)).deleteById(gid);
	}

	@Test
	public void delete_negative() {
		int gid = -567;
		fileGroupService.delete(gid);
		verify(fileGroupRepository, times(0)).deleteById(gid);
	}

	private void add(List<FileGroupEntity> entities, int gid, String name) {
		entities.add(new FileGroupEntity(gid, name, "root"));
	}

}
