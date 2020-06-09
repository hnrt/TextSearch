package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.any;
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

import com.hideakin.textsearch.index.entity.FileEntity;
import com.hideakin.textsearch.index.model.ValuesResponse;
import com.hideakin.textsearch.index.repository.FileRepository;

@SpringBootTest
public class FileServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupService fileGroupService;

	@Mock
	private FileRepository fileRepository;

	@InjectMocks
	private FileService fileService = new FileServiceImpl();

	@Test
	public void getFiles_successful() {
		String group = "quux";
		int gid = 333;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		List<FileEntity> entities = new ArrayList<FileEntity>();
		int fid = 800;
		add(entities, ++fid, "/home/src/quux/foo.java", gid);
		add(entities, ++fid, "/home/src/quux/bar.java", gid);
		add(entities, ++fid, "/home/src/quux/baz.java", gid);
		when(fileRepository.findAllByGid(gid)).thenReturn(entities);
		ValuesResponse rsp = fileService.getFiles(group);
		Assertions.assertEquals(3, rsp.getValues().length);
		Assertions.assertEquals(entities.get(0).getPath(), rsp.getValues()[0]);
		Assertions.assertEquals(entities.get(1).getPath(), rsp.getValues()[1]);
		Assertions.assertEquals(entities.get(2).getPath(), rsp.getValues()[2]);
	}

	@Test
	public void getFiles_nogroup() {
		String group = "quux";
		when(fileGroupService.getGid(group)).thenReturn(-1);
		ValuesResponse rsp = fileService.getFiles(group);
		Assertions.assertEquals(null, rsp.getValues());
	}

	@Test
	public void getFiles_null() {
		String group = "quux";
		int gid = 444;
		when(fileGroupService.getGid(group)).thenReturn(gid);
		when(fileRepository.findAllByGid(gid)).thenReturn(null);
		ValuesResponse rsp = fileService.getFiles(group);
		Assertions.assertEquals(0, rsp.getValues().length);
	}

	@Test
	public void getFid_successful() {
		String path = "/var/lib/xyzzy/helloworld.java";
		int gid = 555;
		List<FileEntity> entities = new ArrayList<FileEntity>();
		int fid = 800;
		add(entities, ++fid, "/var/lib/xyzzy/helloworld.java", gid - 1);
		add(entities, ++fid, "/var/lib/xyzzy/helloworld.java", gid);
		add(entities, ++fid, "/var/lib/xyzzy/helloworld.java", gid + 1);
		when(fileRepository.findAllByPath(path)).thenReturn(entities);
		fid = fileService.getFid(path, gid);
		Assertions.assertEquals(entities.get(1).getFid(), fid);
	}

	@Test
	public void getFid_nogroup() {
		String path = "/var/lib/xyzzy/helloworld.java";
		int gid = 666;
		List<FileEntity> entities = new ArrayList<FileEntity>();
		int fid = 900;
		add(entities, ++fid, "/var/lib/xyzzy/helloworld.java", gid + 1);
		add(entities, ++fid, "/var/lib/xyzzy/helloworld.java", gid + 2);
		add(entities, ++fid, "/var/lib/xyzzy/helloworld.java", gid + 3);
		when(fileRepository.findAllByPath(path)).thenReturn(entities);
		fid = fileService.getFid(path, gid);
		Assertions.assertEquals(-1, fid);
	}

	@Test
	public void getFid_null() {
		String path = "/var/lib/xyzzy/helloworld.java";
		int gid = 777;
		when(fileRepository.findAllByPath(path)).thenReturn(null);
		int fid = fileService.getFid(path, gid);
		Assertions.assertEquals(-1, fid);
	}

	@Test
	public void getFids_successful() {
		int gid = 888;
		List<FileEntity> entities = new ArrayList<FileEntity>();
		int fid = 900;
		add(entities, ++fid, "/var/lib/xyzzy/fred1.java", gid);
		add(entities, ++fid, "/var/lib/xyzzy/fred2.java", gid);
		add(entities, ++fid, "/var/lib/xyzzy/fred3.java", gid);
		when(fileRepository.findAllByGid(gid)).thenReturn(entities);
		List<Integer> fids = fileService.getFids(gid);
		Assertions.assertEquals(3, fids.size());
		Assertions.assertEquals((Integer)entities.get(0).getFid(), fids.get(0));
		Assertions.assertEquals((Integer)entities.get(1).getFid(), fids.get(1));
		Assertions.assertEquals((Integer)entities.get(2).getFid(), fids.get(2));
	}

	@Test
	public void getFids_null() {
		int gid = 888;
		when(fileRepository.findAllByGid(gid)).thenReturn(null);
		List<Integer> fids = fileService.getFids(gid);
		Assertions.assertEquals(0, fids.size());
	}

	@Test
	public void addFile_successful() {
		int maxfid = 987;
		PseudoQuery q = new PseudoQuery(maxfid);
		when(em.createQuery("SELECT max(fid) FROM files")).thenReturn(q);
		FileEntity entity = new FileEntity(maxfid + 1, "/home/waldo/corge.h", 123);
		when(fileRepository.save(any(FileEntity.class))).thenReturn(entity);
		int fid = fileService.addFile(entity.getPath(), entity.getGid());
		Assertions.assertEquals(entity.getFid(), fid);
	}

	@Test
	public void getPath_successful() {
		FileEntity entity = new FileEntity(456, "/home/plugh/thud.cs", 789);
		when(fileRepository.findByFid(entity.getFid())).thenReturn(entity);
		String path = fileService.getPath(entity.getFid(), entity.getGid());
		Assertions.assertEquals(entity.getPath(), path);
	}

	@Test
	public void getPath_groupunmatched() {
		FileEntity entity = new FileEntity(456, "/home/plugh/thud.cs", 789);
		when(fileRepository.findByFid(entity.getFid())).thenReturn(entity);
		String path = fileService.getPath(entity.getFid(), entity.getGid() + 1);
		Assertions.assertEquals(null, path);
	}

	@Test
	public void getPath_null() {
		int fid = 456;
		int gid = 789;
		when(fileRepository.findByFid(fid)).thenReturn(null);
		String path = fileService.getPath(fid, gid);
		Assertions.assertEquals(null, path);
	}

	@Test
	public void delete_successful() {
		int fid = 10;
		List<Integer> fids = new ArrayList<Integer>();
		fids.add(++fid);
		fids.add(++fid);
		fids.add(++fid);
		fids.add(++fid);
		fids.add(++fid);
		fileService.delete(fids);
		verify(fileRepository, times(1)).deleteById(fids.get(0));
		verify(fileRepository, times(1)).deleteById(fids.get(1));
		verify(fileRepository, times(1)).deleteById(fids.get(2));
		verify(fileRepository, times(1)).deleteById(fids.get(3));
		verify(fileRepository, times(1)).deleteById(fids.get(4));
	}

	@Test
	public void delete_empty() {
		List<Integer> fids = new ArrayList<Integer>();
		fileService.delete(fids);
		verify(fileRepository, times(0)).deleteById(any(int.class));
	}

	private void add(List<FileEntity> entities, int fid, String path, int gid) {
		entities.add(new FileEntity(fid, path, gid));
	}
}
