package com.hideakin.textsearch.index.service;

import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;
import static org.mockito.ArgumentMatchers.any;

import java.util.ArrayList;
import java.util.List;
import javax.persistence.EntityManager;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.repository.FileGroupRepository;

@SpringBootTest
public class FileGroupServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupRepository fileGroupRepository;

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
		ValuesResponse rsp = fileGroupService.getGroups();
		Assertions.assertEquals(4, rsp.getValues().length);
		Assertions.assertEquals(entities.get(0).getName(), rsp.getValues()[0]);
		Assertions.assertEquals(entities.get(1).getName(), rsp.getValues()[1]);
		Assertions.assertEquals(entities.get(2).getName(), rsp.getValues()[2]);
		Assertions.assertEquals(entities.get(3).getName(), rsp.getValues()[3]);
	}

	@Test
	public void getGroups_null() {
		when(fileGroupRepository.findAll()).thenReturn(null);
		ValuesResponse rsp = fileGroupService.getGroups();
		Assertions.assertEquals(0, rsp.getValues().length);
	}

	@Test
	public void getGid_successful() {
		FileGroupEntity entity = new FileGroupEntity();
		entity.setGid(123);
		entity.setName("abc");
		when(fileGroupRepository.findByName(entity.getName())).thenReturn(entity);
		int gid = fileGroupService.getGid(entity.getName());
		Assertions.assertEquals(entity.getGid(), gid);
	}

	@Test
	public void getGid_null() {
		when(fileGroupRepository.findByName("xyz")).thenReturn(null);
		int gid = fileGroupService.getGid("xyz");
		Assertions.assertEquals(-1, gid);
	}

	@Test
	public void addGroup_successful() {
		int maxgid = 789;
		PseudoQuery q = new PseudoQuery(maxgid);
		when(em.createQuery("SELECT max(gid) FROM filegroups")).thenReturn(q);
		FileGroupEntity entity = new FileGroupEntity();
		entity.setGid(maxgid + 1);
		entity.setName("def");
		when(fileGroupRepository.save(any(FileGroupEntity.class))).thenReturn(entity);
		int gid = fileGroupService.addGroup(entity.getName());
		Assertions.assertEquals(entity.getGid(), gid);
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
		FileGroupEntity entity = new FileGroupEntity();
		entity.setGid(gid);
		entity.setName(name);
		entities.add(entity);
	}

}
