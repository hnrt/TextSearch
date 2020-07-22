package com.hideakin.textsearch.index.service;

import static org.mockito.ArgumentMatchers.anyInt;
import static org.mockito.ArgumentMatchers.eq;
import static org.mockito.Mockito.times;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

import java.util.ArrayList;
import java.util.Arrays;

import javax.persistence.EntityManager;

import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.springframework.boot.test.context.SpringBootTest;

import com.hideakin.textsearch.index.data.SearchOptions;
import com.hideakin.textsearch.index.entity.FileGroupEntity;
import com.hideakin.textsearch.index.entity.TextEntity;
import com.hideakin.textsearch.index.model.TextDistribution;
import com.hideakin.textsearch.index.repository.FileGroupRepository;
import com.hideakin.textsearch.index.repository.FileRepository;
import com.hideakin.textsearch.index.repository.TextRepository;

@SpringBootTest
public class IndexServiceTests {

	@Mock
	private EntityManager em;

	@Mock
	private FileGroupRepository fileGroupRepository;

	@Mock
	private FileRepository fileRepository;

	@Mock
	private TextRepository textRepository;

	@InjectMocks
	private IndexService indexService = new IndexServiceImpl();

	@Test
	public void findText_nogroup() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(null);
		TextDistribution[] hits = indexService.findText("xyzzy", "FOO", SearchOptions.Exact, 1, 0);
		Assertions.assertEquals(null, hits);
		verify(fileGroupRepository, times(1)).findByName("xyzzy");
		verify(textRepository, times(0)).findByTextAndGid(eq("FOO"), anyInt());
	}

	@Test
	public void findText_nohit() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(3, "xyzzy"));
		when(textRepository.findByTextAndGid("FOO", 3)).thenReturn(null);
		TextDistribution[] hits = indexService.findText("xyzzy", "FOO", SearchOptions.Exact, 1, 0);
		Assertions.assertEquals(0, hits.length);
		verify(fileGroupRepository, times(1)).findByName("xyzzy");
		verify(textRepository, times(1)).findByTextAndGid("FOO", 3);
	}

	@Test
	public void findText_exact() {
		when(fileGroupRepository.findByName("xyzzy")).thenReturn(new FileGroupEntity(4, "xyzzy"));
		when(textRepository.findByTextAndGid("FOO", 4)).thenReturn(
				new TextEntity("FOO", 4, new byte[] { 3, 1, 11, 5, 2, 13, 17, 7, 3, 19, 23, 29 }));
		TextDistribution[] hits = indexService.findText("xyzzy", "FOO", SearchOptions.Exact, 1, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("xyzzy");
		verify(textRepository, times(1)).findByTextAndGid("FOO", 4);
	}

	@Test
	public void findText_contains() {
		when(fileGroupRepository.findByName("corge")).thenReturn(new FileGroupEntity(4, "corge"));
		when(em.createQuery("SELECT t FROM texts t WHERE t.text LIKE :expr AND t.gid = :gid ORDER BY t.text")).thenReturn(new PseudoQuery(
				new ArrayList<TextEntity>(Arrays.asList(
						new TextEntity("XBARX", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("YBARY", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				))));
		TextDistribution[] hits = indexService.findText("corge", "BAR", SearchOptions.Contains, 256, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("corge");
	}

	@Test
	public void findText_startsWith() {
		when(fileGroupRepository.findByName("corge")).thenReturn(new FileGroupEntity(4, "corge"));
		when(em.createQuery("SELECT t FROM texts t WHERE t.text LIKE :expr AND t.gid = :gid ORDER BY t.text")).thenReturn(new PseudoQuery(
				new ArrayList<TextEntity>(Arrays.asList(
						new TextEntity("BAZX", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("BAZY", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				))));
		TextDistribution[] hits = indexService.findText("corge", "BAZ", SearchOptions.StartsWith, 256, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("corge");
	}

	@Test
	public void findText_endsWith() {
		when(fileGroupRepository.findByName("corge")).thenReturn(new FileGroupEntity(4, "corge"));
		when(em.createQuery("SELECT t FROM texts t WHERE t.text LIKE :expr AND t.gid = :gid ORDER BY t.text")).thenReturn(new PseudoQuery(
				new ArrayList<TextEntity>(Arrays.asList(
						new TextEntity("XTHUD", 4, new byte[] { 3, 1, 11, 5, 1, 17, 7, 1, 23 }),
						new TextEntity("YTHUD", 4, new byte[] { 5, 1, 13, 7, 2, 19, 29 })
				))));
		TextDistribution[] hits = indexService.findText("corge", "THUD", SearchOptions.EndsWith, 256, 0);
		Assertions.assertEquals(3, hits.length);
		Assertions.assertEquals(3, hits[0].getFid());
		Assertions.assertEquals(1, hits[0].getPositions().length);
		Assertions.assertEquals(11, hits[0].getPositions()[0]);
		Assertions.assertEquals(5, hits[1].getFid());
		Assertions.assertEquals(2, hits[1].getPositions().length);
		Assertions.assertEquals(13, hits[1].getPositions()[0]);
		Assertions.assertEquals(17, hits[1].getPositions()[1]);
		Assertions.assertEquals(7, hits[2].getFid());
		Assertions.assertEquals(3, hits[2].getPositions().length);
		Assertions.assertEquals(19, hits[2].getPositions()[0]);
		Assertions.assertEquals(23, hits[2].getPositions()[1]);
		Assertions.assertEquals(29, hits[2].getPositions()[2]);
		verify(fileGroupRepository, times(1)).findByName("corge");
	}

}
